using System.Diagnostics;

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
  private readonly IConfiguration _configuration;
  private readonly HttpClient _httpClient;

  public ChatController(ChatDbContext chatDb, ILogger<ChatController> logger, IConfiguration configuration, HttpClient httpClient)
  {
    _chatDb = chatDb;
    _logger = logger;
    _configuration = configuration;
    _httpClient = httpClient;
    _logger.LogInformation($"ChatController has been created here is your base address {httpClient.BaseAddress} ");
  }

  [HttpPost]
  public async Task<ActionResult> AddNewMessage([FromBody] MessageWithImages message)
  {
    _logger.LogInformation("Adding message to database");
    var currentSpan = Activity.Current;
    try
    {
      _logger.LogInformation($"Here is the image count {message.Images.Count()}");
      currentSpan?.SetTag(DiagnosticNames.messageImagePresent, false);
      await _chatDb.Messages.AddAsync(message.Message);
      await _chatDb.SaveChangesAsync();

      if (message.Images.Count() > 0)
      {
        var savedPictures = new List<Picture>();
        currentSpan?.SetTag(DiagnosticNames.messageImagePresent, true);
        DiagnosticConfig.messageWithImageCount.Add(1);

        foreach (var imageURI in message.Images)
        {
          var imageRequest = new SaveImageRequest
          {
            imageURI = imageURI,
            messageId = message.Message.Id.ToString()
          };
          //make calls to the image api
          await _httpClient.PostAsJsonAsync<SaveImageRequest>("api/image", imageRequest);
        }
      }


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
    var messages = await _chatDb.Messages
      .Include(m => m.Pictures)
      .ToListAsync();

    _logger.LogInformation("Retrieving all messages");

    if (messages == null)
    {
      DiagnosticConfig.retrieveAllMessagesFailedCount.Add(1);

      return StatusCode(500, "No Message found in database");
    }

    var messagesInOrder = messages.OrderBy(m => m.CreatedAt).ToList();
    var messagesWithImages = new List<MessageWithImages>();
    foreach (var m in messagesInOrder)
    {
      var imagesForMessage = new List<string>();
      foreach (var image in m.Pictures)
      {
        string imageURI = await _httpClient.GetStringAsync($"api/image?imageId={image.Id}");
        imagesForMessage.Add(imageURI);
      }

      messagesWithImages.Add(new MessageWithImages
      {
        Message = m,
        Images = imagesForMessage
      });
    }
    _logger.LogInformation("Retrieved all messages here is the message");
    return messagesWithImages.ToList();

  }
}