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

  public async Task SendMessageAsync(MessageWithImages message)
  {
    await _httpClient.PostAsJsonAsync("api/chat", message);
  }

  public async Task<List<Message>> GetMessagesAsync()
  {
    return await _httpClient.GetFromJsonAsync<List<Message>>("api/chat") ?? throw new Exception("No data returned from the api");
  }
}