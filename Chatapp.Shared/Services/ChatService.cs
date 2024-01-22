using System.Net.Http.Json;

using Chatapp.Shared.Entities;
using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Simple_Models;

namespace Chatapp.Shared.Services;
public class ChatService : IChatService
{
  private readonly HttpClient _httpClient;

  public ChatService(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public Task<List<Message>> GetMessagesAsync()
  {
    throw new NotImplementedException();
  }

  public Task SendMessageAsync(MessageWithImages message)
  {
    return _httpClient.PostAsJsonAsync("api/chat", message);
  }
}