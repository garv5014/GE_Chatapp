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
  public async Task<ActionResult<IEnumerable<MessageWithImages>>> RetrieveAllMessages()
  {
    var message = await _chatDb.Messages
      .Include(m => m.Pictures)
      .ToListAsync();

    _logger.LogInformation("Retrieving all messages");

    if (message == null)
    {
      DiagnosticConfig.retrieveAllMessagesFailedCount.Add(1);

      return StatusCode(500, "No Message found in database");
    }

    var messagesInOrder = message.OrderBy(m => m.CreatedAt).ToList();

    IEnumerable<MessageWithImages> messagesWithImages = messagesInOrder.Select(m => new MessageWithImages
    {
      Message = m,
      Images = m.Pictures.Select((p) => RetrieveImage(p.Id)).ToList()

    });
    _logger.LogInformation("Retrieved all messages here is the message");
    return messagesWithImages.ToList();
  }

  public string RetrieveImage(int id)
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