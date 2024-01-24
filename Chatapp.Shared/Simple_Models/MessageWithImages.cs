using Chatapp.Shared.Entities;
namespace Chatapp.Shared.Simple_Models;
public class MessageWithImages
{
  public List<string> Images { get; set; } = new List<string>();

  public Message Message { get; set; } = new Message();
}