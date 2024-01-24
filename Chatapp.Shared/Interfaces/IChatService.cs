using Chatapp.Shared.Simple_Models;

namespace Chatapp.Shared.Interfaces;

public interface IChatService
{
  Task<List<MessageWithImages>> GetMessagesAsync();
  Task SendMessageAsync(MessageWithImages message);
}