using System.Net.Http.Json;

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

  public async Task<List<ChatMessage>> GetMessagesAsync()
  {
    return await _httpClient.GetFromJsonAsync<List<ChatMessage>>("api/chat");
  }

  public async Task SendMessageAsync(ChatMessage message)
  {
    await _httpClient.PostAsJsonAsync("api/chat", message);
  }
}