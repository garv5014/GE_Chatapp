using Chatapp.Shared.Entities;
using Chatapp.Shared.Simple_Models;

namespace Chatapp.Shared.Interfaces;

public interface IChatService
{
  Task<List<Message>> GetMessagesAsync();
  Task SendMessageAsync(MessageWithImages message);
}