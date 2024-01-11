using Microsoft.AspNetCore.Mvc;

namespace GE_Chatapp.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
  public List<Message> Messages { get; set; } = new List<Message>();
  public ChatController()
  {

  }

  [HttpPut]
  public void Put(Message message)
  {
    Messages.Add(message);
  }

  [HttpGet]
  public List<Message> Get()
  {
    return Messages.OrderBy(m => m.Timestamp).ToList();
  }
}

public class Message
{
  public string Text { get; set; }
  public string User { get; set; }
  public DateTime Timestamp { get; set; }
}