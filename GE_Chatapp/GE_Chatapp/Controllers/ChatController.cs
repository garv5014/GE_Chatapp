using System.Diagnostics;

using Chatapp.Shared;
using Chatapp.Shared.Entities;
using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Services;
using Chatapp.Shared.Simple_Models;
using Chatapp.Shared.Telemetry;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;


namespace GE_Chatapp.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
  private readonly ChatDbContext _chatDb;
  private readonly ILogger _logger;
  private readonly IConfiguration _configuration;
  private readonly IFileAPIService _fileService;
  private readonly HubConnection? _hubConnection;

  public ChatController(ChatDbContext chatDb, ILogger<ChatController> logger, IConfiguration configuration, IFileAPIService fileService, SignalREnv signalREnv)
  {
    _chatDb = chatDb;
    _logger = logger;
    _configuration = configuration;
    _fileService = fileService;
    _hubConnection = new HubConnectionBuilder()
    .WithUrl(signalREnv.chatHubURL)
    .Build();
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
          await _fileService.PostImageToFileApi(imageRequest);
        }
      }
      if (_hubConnection is not null)
      {
        await _hubConnection.SendAsync("UpdateMessages");
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
  public async Task<ActionResult<IEnumerable<Message>>> RetrieveAllMessages()
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

    var messagesInOrder = messages.OrderByDescending(m => m.CreatedAt).ToList();

    return messagesInOrder.ToList();
  }

  [HttpGet("messagesAfter")]
  public async Task<ActionResult<IEnumerable<Message>>> RetrieveMessagesAfterDate([FromQuery] DateTime dateAfter)
  {
    var messages = await _chatDb.Messages
      .Where(m => m.CreatedAt > dateAfter)
      .Include(m => m.Pictures)
      .ToListAsync();

    _logger.LogInformation($"Retrieving all messages after {dateAfter}");

    if (messages == null)
    {
      DiagnosticConfig.retrieveAllMessagesFailedCount.Add(1);

      return StatusCode(500, "No Message found in database");
    }

    var messagesInOrder = messages.Where(x => x.CreatedAt.AddTicks(-x.CreatedAt.Ticks % TimeSpan.TicksPerSecond) > dateAfter)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToList();

    return messagesInOrder.ToList();
  }
}