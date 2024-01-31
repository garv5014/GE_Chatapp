using Chatapp.Shared;
using Chatapp.Shared.Entities;
using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Simple_Models;

using Microsoft.AspNetCore.Mvc;

namespace FileAPI.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class ImageController : ControllerBase
{
  private readonly ChatDbContext _chatDb;
  private readonly ILogger _logger;
  private readonly IConfiguration _configuration;
  private readonly IFileService _fileService;

  public ImageController(ChatDbContext chatDb,
    ILogger<ImageController> logger,
    IConfiguration configuration,
    IFileService fileService)
  {
    _chatDb = chatDb;
    _logger = logger;
    _configuration = configuration;
    _fileService = fileService;
  }

  [HttpPost("save")]
  public async Task<ActionResult<string>> SaveImageToDriveAndDatabase(SaveImageRequest imageRequest)
  {
    try
    {
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
  public Task<ActionResult<string>> RetrieveImageFromDrive([FromRoute] int imageId)
  {
    try
    {
      var targetPicture = _chatDb.Pictures.Find(imageId);
      // Construct the file path
      string filePath = $"/app/images/{targetPicture?.NameOfFile ?? throw new FileNotFoundException("Target image was not found")}.png";

      return Task.FromResult<ActionResult<string>>(_fileService.RetrieveImageFromDrive(filePath));
    }
    catch (Exception ex)
    {
      _logger.LogError($"There was an error saving your image {ex.Message}");
      return Task.FromResult<ActionResult<string>>(StatusCode(500, "Internal server error in retrieving image"));
    }
  }
}