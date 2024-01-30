using Chatapp.Shared;
using Chatapp.Shared.Entities;
using Chatapp.Shared.Simple_Models;
using Chatapp.Shared.Telemetry;

using ImageMagick;

using Microsoft.AspNetCore.Mvc;

namespace FileAPI.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class ImageController : ControllerBase
{
  private readonly ChatDbContext _chatDb;
  private readonly ILogger _logger;
  private readonly IConfiguration _configuration;

  public ImageController(ChatDbContext chatDb, ILogger<ImageController> logger, IConfiguration configuration)
  {
    _chatDb = chatDb;
    _logger = logger;
    _configuration = configuration;
  }

  [HttpPost]
  public async Task<ActionResult<string>> SaveImageToDrive(SaveImageRequest imageRequest)
  {
    try
    {
      // get base64 string from query
      var picture = new Picture();
      picture.NameOfFile = Guid.NewGuid().ToString();
      var image = imageRequest.imageURI.Replace("data:image/png;base64,", "");

      // save to drive
      _logger.LogInformation($"Saving image {picture.NameOfFile} to database {image}");

      byte[] bytes = Convert.FromBase64String(image);
      string filePath = Path.Combine("/app/images", picture.NameOfFile + ".png");

      await System.IO.File.WriteAllBytesAsync(filePath, bytes); // Write the file to the filesystem

      if (_configuration["CompressImages"] == "true")
      {
        using (var imageCompression = DiagnosticConfig.Source.StartActivity(DiagnosticNames.imageCompression))
        {
          imageCompression?.SetTag(DiagnosticNames.imageCompressionId, picture.NameOfFile);

          var optimizer = new ImageOptimizer();
          _logger.LogInformation($"Compressing image {picture.NameOfFile} to filesystem");
          optimizer.Compress(filePath);
        }
      }
      // get message id from query
      picture.BelongsTo = int.Parse(imageRequest.messageId);
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

  [HttpGet]
  public async Task<ActionResult<string>> RetrieveImageFromDrive([FromQuery] string imageId)
  {
    try
    {
      return RetrieveImage(int.Parse(imageId));
    }
    catch (Exception ex)
    {
      _logger.LogError($"There was an error saving your image {ex.Message}");
      return StatusCode(500, "Internal server error in retrieving image");
    }
  }

  private string RetrieveImage(int id)
  {
    var targetPicture = _chatDb.Pictures.Find(id);
    // Construct the file path
    string filePath = $"/app/images/{targetPicture?.NameOfFile ?? throw new FileNotFoundException("Target image was not found")}.png";

    // Check if the file exists
    if (!System.IO.File.Exists(filePath))
    {
      throw new FileNotFoundException("The image file was not found.", filePath);
    }

    // Read the file into a byte array
    byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);

    // Convert the byte array to a Base64 string
    string base64String = Convert.ToBase64String(imageBytes);

    // Construct the Data URI
    return $"data:image/png;base64,{base64String}";
  }
}
