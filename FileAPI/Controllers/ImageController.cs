using Chatapp.Shared;
using Chatapp.Shared.Entities;
using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Simple_Models;

using FileAPI.Options;

using Microsoft.AspNetCore.Mvc;

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
    try
    {
      await Task.Delay(_fileAPIOptions.APIDelayInSeconds * 1000);
      // get base64 string from query
      var picture = new Picture();
      var image = imageRequest.imageURI.Replace("data:image/png;base64,", "");

      var savedFileName = _fileService.SaveImageToDrive(image);
      // get message id from query
      picture.BelongsTo = int.Parse(imageRequest.messageId);
      picture.NameOfFile = savedFileName;
      // save to database
      await _chatDb.Pictures.AddAsync(picture);
      await _chatDb.SaveChangesAsync();
      _logger.LogInformation($"Image {picture.NameOfFile} saved to database");
      return Ok();
    }
    catch (Exception ex)
    {
      _logger.LogError($"There was an error saving your image {ex.Message}");
      return StatusCode(500, "Internal server error in saving image to database");
    }
  }

  [HttpGet("{imageId}")]
  public async Task<ActionResult<string>> RetrieveImageFromDrive([FromRoute] int imageId)
  {
    try
    {
      await Task.Delay(_fileAPIOptions.APIDelayInSeconds * 1000);
      var targetPicture = _chatDb.Pictures.Find(imageId);
      // Construct the file path
      string filePath = $"/app/images/{targetPicture?.NameOfFile ?? throw new FileNotFoundException("Target image was not found")}.png";

      return _fileService.RetrieveImageFromDrive(filePath);
    }
    catch (Exception ex)
    {
      _logger.LogError($"There was an error saving your image {ex.Message}");
      return StatusCode(500, "Internal server error in retrieving image");
    }
  }
}