using Chatapp.Shared;
using Chatapp.Shared.Entities;
using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Simple_Models;
using Chatapp.Shared.Telemetry;

using FileAPI.Options;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileAPI.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class ImageController : ControllerBase
{
  private readonly ChatDbContext _chatDb;
  private readonly ILogger _logger;
  private readonly IFileService _fileService;
  private readonly FileAPIOptions _fileAPIOptions;
  private readonly IRedisService _redisService;

  public ImageController(ChatDbContext chatDb,
    ILogger<ImageController> logger,
    IFileService fileService,
    FileAPIOptions fileAPIOptions,
    IRedisService redisService
    )
  {
    _chatDb = chatDb;
    _logger = logger;
    _fileService = fileService;
    _fileAPIOptions = fileAPIOptions;
    _redisService = redisService;
  }

  [HttpPost("save")]
  public async Task<ActionResult<string>> SaveImageToDriveAndDatabase(SaveImageRequest imageRequest)
  {
    using var saveImageTransaction = _chatDb.Database.BeginTransaction();

    try
    {
      await Task.Delay(_fileAPIOptions.APIDelayInSeconds * 1000);

      // get base64 string from query
      var picture = new Picture();
      var image = imageRequest.imageURI.Replace("data:image/png;base64,", "");
      var savedFileName = _fileService.SaveImageToDrive(image);

      // store image in redis
      await _redisService.StoreValue(savedFileName, image);

      // get message id from query
      picture.BelongsTo = int.Parse(imageRequest.messageId);
      picture.NameOfFile = savedFileName;

      // save to database
      await _chatDb.Pictures.AddAsync(picture);
      await _chatDb.SaveChangesAsync();
      await saveImageTransaction.CommitAsync();

      var pictureLookup = new PictureLookup
      {
        PictureId = picture.Id,
        MachineName = _fileAPIOptions.ServiceName
      };

      await _chatDb.PictureLookups.AddAsync(pictureLookup);
      await _chatDb.SaveChangesAsync();

      _logger.LogInformation($"Image {picture.NameOfFile} saved to database");
      DiagnosticConfig.totalPhotos.Add(1);
      return Ok();
    }
    catch (Exception ex)
    {
      await saveImageTransaction.RollbackAsync();

      _logger.LogError($"There was an error saving your image {ex.Message}");
      return StatusCode(500, "Internal server error in saving image to database");
    }
  }

  [HttpPost("replicate/{imageId}")]
  public async Task<ActionResult> ReplicateImage([FromRoute] int imageId)
  {
    try
    {
      var targetPicture = await _chatDb.Pictures.FindAsync(imageId);
      // check if picture belongs to this service
      var pictureLookup = await _chatDb.PictureLookups.FirstOrDefaultAsync(p => p.PictureId == imageId);
      if (pictureLookup == null || string.IsNullOrEmpty(targetPicture?.NameOfFile))
      {
        _logger.LogError($"Image {targetPicture?.NameOfFile} could not be found in database");
        return StatusCode(404, "Image couldn't be found in other services");
      }

      if (pictureLookup.MachineName != _fileAPIOptions.ServiceName)
      {

        // get the service name from the picture lookup
        _logger.LogInformation($"Image {targetPicture?.NameOfFile} does not belong to this service");
        // call the other service to get the image
        var client = new HttpClient()
        {
          BaseAddress = new Uri($"http://{pictureLookup.MachineName}:8080")
        };

        var response = await client.GetStringAsync($"/api/image/{imageId}");
        var imageBase64 = response.Replace("data:image/png;base64,", "");

        var successfullyCopied = _fileService.SaveCopyToDrive(imageBase64, targetPicture?.NameOfFile);

        if (!successfullyCopied)
        {
          return StatusCode(500, "Internal server error in replicating image");
        }

        var newLookup = new PictureLookup
        {
          PictureId = imageId,
          MachineName = _fileAPIOptions.ServiceName
        };

        await _chatDb.PictureLookups.AddAsync(newLookup);
        await _chatDb.SaveChangesAsync();

        // save to database new entry for replicated image for this service 
        DiagnosticConfig.backedUpPhotos.Add(1, new KeyValuePair<string, object>("service", _fileAPIOptions.ServiceName));
        return Ok();
      }

      return StatusCode(500, "Image is already on this machine");
    }
    catch (Exception ex)
    {
      _logger.LogError($"There was an error saving your image {ex.Message}");
      return StatusCode(500, "Internal server error in replicating image");
    }
  }

  [HttpGet("{imageId}")]
  public async Task<ActionResult<string>> RetrieveImageFromDrive([FromRoute] int imageId)
  {
    try
    {
      await Task.Delay(_fileAPIOptions.APIDelayInSeconds * 1000);
      var targetPicture = _chatDb.Pictures.Find(imageId);
      // check if picture belongs to this service
      var pictureLookups = await _chatDb.PictureLookups.Where(p => p.PictureId == imageId).ToListAsync();
      if (pictureLookups == null)
      {
        _logger.LogError($"Image {targetPicture?.NameOfFile} could not be found in database");
        return StatusCode(404, "Image couldn't be found in other services");
      }

      Random random = new Random();
      var ranNumIndex = random.Next(0, pictureLookups.Count);
      var machineName = pictureLookups[ranNumIndex].MachineName;
      _logger.LogInformation($"Get Image {targetPicture?.NameOfFile} from {machineName}");
      if (machineName != _fileAPIOptions.ServiceName)
      {
        // get the service name from the picture lookup
        _logger.LogInformation($"Image {targetPicture?.NameOfFile} does not belong to this service");
        // call the other service to get the image
        var client = new HttpClient()
        {
          BaseAddress = new Uri($"http://{machineName}:8080")
        };

        var response = await client.GetStringAsync($"/api/image/{imageId}");
        if (response == null)
        {
          return StatusCode(404, "Image couldn't be found in other services");
        }
        return response;
      }

      if (_redisService.KeyExists(targetPicture?.NameOfFile))
      {
        _logger.LogInformation($"Image {targetPicture?.NameOfFile} retrieved from redis");
        var cachedValue = _redisService.RetrieveKeyValue(targetPicture?.NameOfFile).ToString();
        return $"data:image/png;base64,{cachedValue}";
      }
      // Construct the file path
      string filePath = $"/app/images/{targetPicture?.NameOfFile ?? throw new FileNotFoundException("Target image was not found")}.png";

      var base64 = _fileService.RetrieveImageFromDrive(filePath);
      return await MakeDataUri(base64, "png");
    }
    catch (Exception ex)
    {
      _logger.LogError($"There was an error saving your image {ex.Message}");
      return StatusCode(500, "Internal server error in retrieving image");
    }
  }

  public Task<string> MakeDataUri(string base64Image, string imageType)
  {
    return Task.FromResult($"data:image/{imageType};base64,{base64Image}");
  }
}