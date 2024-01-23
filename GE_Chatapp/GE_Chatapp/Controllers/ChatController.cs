using Chatapp.Shared;
using Chatapp.Shared.Entities;
using Chatapp.Shared.Simple_Models;
using Chatapp.Shared.Telemetry;


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace GE_Chatapp.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
  private readonly ChatDbContext _chatDb;
  private readonly ILogger _logger;

  public ChatController(ChatDbContext chatDb, ILogger<ChatController> logger)
  {
    _chatDb = chatDb;
    _logger = logger;
  }

  [HttpPost]
  public async Task<ActionResult> AddNewMessage([FromBody] MessageWithImages message)
  {
    _logger.LogInformation("Adding message to database");
    try
    {
      // make new message object
      if (message.Message.MessageText == "FAILED")
      {
        _logger.LogInformation("Failed to add message to database");
        throw new Exception("Message failed");
      }
      // for each image make a unique file name 
      // add that name to the message object in the picture
      // save the picture to the image folder
      List<Picture> savedPictures = new List<Picture>();
      _logger.LogInformation($"Here is the image count {message.Images.Count()}");
      if (message.Images.Count() > 0)
      {
        foreach (var imageURI in message.Images)
        {
          var picture = new Picture();
          picture.NameOfFile = Guid.NewGuid().ToString();
          var image = imageURI.Replace("data:image/png;base64,", "");
          _logger.LogInformation($"Saving image {picture.NameOfFile} to database");

          byte[] bytes = Convert.FromBase64String(image);
          string filePath = Path.Combine("/app/images", picture.NameOfFile + ".png");
          await System.IO.File.WriteAllBytesAsync(filePath, bytes); // Write the file to the filesystem
          savedPictures.Add(picture);
        }
      }

      await _chatDb.Messages.AddAsync(message.Message);
      await _chatDb.SaveChangesAsync();
      if (savedPictures.Count() > 0)
      {
        var savePictures = savedPictures.Select(p => p.BelongsTo = message.Message.Id).ToList();
      }
      await _chatDb.Pictures.AddRangeAsync(savedPictures);
      await _chatDb.SaveChangesAsync();

      DiagnosticConfig.messageCount.Add(1);
    }
    catch (Exception e)
    {
      _logger.LogError(e.Message);
      DiagnosticConfig.newMessageFailedCount.Add(1);
      return StatusCode(500, "Internal server error");
    }

    return Ok();
  }

  [HttpGet]
  public async Task<ActionResult<List<Message>>> RetrieveAllMessages()
  {
    var message = await _chatDb.Messages.ToListAsync();

    _logger.LogInformation("Retrieving all messages");

    if (message == null)
    {
      DiagnosticConfig.retrieveAllMessagesFailedCount.Add(1);

      return StatusCode(500, "Messages are null");
    }

    return message.OrderBy(m => m.CreatedAt).ToList();
  }

  [HttpGet("image/{id}")]
  public async Task<ActionResult> RetrieveImage(int id)
  {
    var picture = await _chatDb.Pictures.FirstOrDefaultAsync(p => p.Id == id);

    _logger.LogInformation("Retrieving image");

    if (picture == null)
    {
      return NotFound("Image not found");
    }

    var filePath = Path.Combine("/image", picture.NameOfFile);
    _logger.LogInformation("File path: " + filePath);
    if (!System.IO.File.Exists(filePath))
    {
      return NotFound("File does not exist");
    }

    var contentType = "image/jpeg"; // Or dynamically determine the MIME type based on the file extension
    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
    return File(fileBytes, contentType, picture.NameOfFile);
  }
}