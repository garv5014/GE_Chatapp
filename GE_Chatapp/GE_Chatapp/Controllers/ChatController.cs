using Chatapp.Shared;
using Chatapp.Shared.Entities;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GE_Chatapp.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
  private readonly ChatDbContext _chatDb;

  public ChatController(ChatDbContext chatDb)
  {
    _chatDb = chatDb;
  }

  [HttpPost]
  public async Task<ActionResult> AddNewMessage([FromBody] Message message)
  {
    await _chatDb.Messages.AddAsync(message);
    await _chatDb.SaveChangesAsync();
    return Ok();
  }

  [HttpGet]
  public async Task<ActionResult<List<Message>>> RetrieveAllMessages()
  {
    var message = await _chatDb.Messages.ToListAsync();
    return message.OrderBy(m => m.CreatedAt).ToList();
  }
}