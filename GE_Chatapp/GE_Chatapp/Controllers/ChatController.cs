using Chatapp.Shared;
using Chatapp.Shared.Entities;

using Microsoft.AspNetCore.Mvc;

namespace GE_Chatapp.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
  private readonly ChatDbContext _chatDb;

  public List<Message> Messages { get; set; } = new List<Message>();

  public ChatController(ChatDbContext chatDb)
  {
    _chatDb = chatDb;
  }

  [HttpPost]
  public async Task<ActionResult> AddNewMessage([FromBody] Message message)
  {
    await _chatDb.Messages.AddAsync(message);
    return Ok();
  }

  [HttpGet]
  public async Task<ActionResult<List<Message>>> RetrieveAllMessages()
  {
    Messages.OrderBy(m => m.CreatedAt).ToList();
    return Messages;
  }
}