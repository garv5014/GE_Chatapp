using Chatapp.Shared;
using Chatapp.Shared.Entities;
using Chatapp.Shared.Telemetry;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GE_Chatapp.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
  private readonly ChatDbContext _chatDb;
  // private readonly ILogger _logger;

  public ChatController(ChatDbContext chatDb)
  {
    _chatDb = chatDb;
    // _logger = logger;
  }

  [HttpPost]
  public async Task<ActionResult> AddNewMessage([FromBody] Message message)
  {
    DiagnosticConfig.messageCount.Add(1);
    // _logger.LogInformation("Adding message to database");
    try
    {
      if (message.MessageText == "FAILED")
      {
        throw new Exception("Message failed");
      }

      await _chatDb.Messages.AddAsync(message);
      await _chatDb.SaveChangesAsync();
    }
    catch
    {
      DiagnosticConfig.newMessageFailedCount.Add(1);

      return StatusCode(500, "Internal server error");
    }

    return Ok();
  }

  [HttpGet]
  public async Task<ActionResult<List<Message>>> RetrieveAllMessages()
  {
    var message = await _chatDb.Messages.ToListAsync();

    // _logger.LogInformation("Retrieving all messages");

    if (message == null)
    {
      DiagnosticConfig.retrieveAllMessagesFailedCount.Add(1);

      return StatusCode(500, "Messages are null");
    }

    return message.OrderBy(m => m.CreatedAt).ToList();
  }
}