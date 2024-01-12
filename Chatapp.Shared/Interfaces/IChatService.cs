using Chatapp.Shared.Entities;

namespace Chatapp.Shared.Interfaces;

public interface IChatService
{
  Task<List<Message>> GetMessagesAsync();
  Task SendMessageAsync(Message message);
}