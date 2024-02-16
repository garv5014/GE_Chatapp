using System.Net.Http.Json;

using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Simple_Models;

namespace Chatapp.Shared.Services;

public class FileApiService : IFileAPIService
{
  private readonly HttpClient _httpClient;

  public FileApiService(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }
  public async Task PostImageToFileApi(SaveImageRequest imageRequest)
  {
    await _httpClient.PostAsJsonAsync<SaveImageRequest>("/api/image/save", imageRequest);
  }

  public async Task<string> RetrieveImageFromFileApi(string imageId)
  {
    Console.WriteLine($"here is the httpclient {_httpClient.BaseAddress}");
    return await _httpClient.GetStringAsync($"/api/image/{imageId}");
  }
}