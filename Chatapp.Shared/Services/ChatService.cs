using System.Net.Http.Json;

using Chatapp.Shared.Entities;
using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Simple_Models;

namespace Chatapp.Shared.Services;
public class ChatService : IChatService
{
  private readonly HttpClient _httpClient;
  private readonly IFileAPIService _fileService;

  public ChatService(HttpClient httpClient, IFileAPIService fileService)
  {
    _httpClient = httpClient;
    _fileService = fileService;
  }

  public async Task SendMessageAsync(MessageWithImages message)
  {
    await _httpClient.PostAsJsonAsync("/api/chat", message);
  }

  public async Task<List<MessageWithImages>> GetMessagesAsync()
  {
    var messages = await _httpClient.GetFromJsonAsync<List<Message>>("/api/chat") ?? throw new Exception("No data returned from the api");
    // for each message, get the images for the message
    var messagesWithImages = new List<MessageWithImages>();
    foreach (var message in messages)
    {
      var images = new List<string>();
      foreach (var image in message.Pictures)
      {
        images.Add(await _fileService.RetrieveImageFromFileApi(image.Id.ToString()));
      }
      messagesWithImages.Add(new MessageWithImages
      {
        Message = message,
        Images = images
      });
    }
    return messagesWithImages;
  }

}