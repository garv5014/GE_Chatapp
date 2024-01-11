using Chatapp.Shared.Simple_Models;

namespace Chatapp.Shared.Interfaces;

public interface IChatService
{
  Task<List<ChatMessage>> GetMessagesAsync();
  Task SendMessageAsync(ChatMessage message);
}
