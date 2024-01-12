using System.Net.Http.Json;

using Chatapp.Shared.Entities;
using Chatapp.Shared.Interfaces;

namespace Chatapp.Shared.Services;
public class ChatService : IChatService
{
  private readonly HttpClient _httpClient;

  public ChatService(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<List<Message>> GetMessagesAsync()
  {
    Console.WriteLine("GetMessagesAsync", _httpClient.BaseAddress);
    return await _httpClient.GetFromJsonAsync<List<Message>>("api/chat");
  }

  public async Task SendMessageAsync(Message message)
  {
    await _httpClient.PostAsJsonAsync("api/chat", message);
  }
}