using Chatapp.Shared.Entities;
namespace Chatapp.Shared.Simple_Models;
public class MessageWithImages
{
  public List<string> Images { get; set; } = new List<string>();

  public Guid ClientId { get; set; }

  public int EventCount { get; set; }

  public Message Message { get; set; } = new Message();
}