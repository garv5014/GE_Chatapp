using System.Net.Http.Json;

using Chatapp.Shared;
using Chatapp.Shared.Entities;
using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Simple_Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Integration_Test;

public class SaveImageToDriveTest : IClassFixture<FileApiWebApplicationFactory>
{
  private readonly IServiceScope _scope;
  private readonly IFileService _fileService;
  private readonly ChatDbContext _dbcontext;
  private readonly HttpClient _httpClient;
  public SaveImageToDriveTest(FileApiWebApplicationFactory factory)
  {
    _scope = factory.Services.CreateScope();
    _fileService = _scope.ServiceProvider.GetRequiredService<IFileService>();
    _dbcontext = _scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    _httpClient = _scope.ServiceProvider.GetRequiredService<HttpClient>();
  }

  [Fact]
  public async void SaveImageToDriveTestAsync()
  {
    var message = new Message
    {
      MessageText = "Hello World",
      CreatedAt = DateTime.Now,
      Username = "Test User"
    };

    _dbcontext.Messages.Add(message);
    await _dbcontext.SaveChangesAsync();
    var req = new SaveImageRequest
    {
      imageURI = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAGQAAABkCAIAAAD/gAIDAAACJUlEQVR4nO2c227rMAwElYP+/y/7PBAQCrS1ORR19c5zGnMnSycNWn2u6yrCx7/ZA+yEZAEkCyBZAMkCSBZAsgCSBZAswNfsAUop5VM+noddZfIvG9NkOQX99SNTxI2WFXB0/zwjrQ2SleXo5pkHWOsuq5+mXy/UVVlHWcM0/bxoJ2W9PjpMMdX76vnNmqup0qNiyc1axFQld55MWauZMhKnylnDNTVVslYyoVmLm6q0z9kqaxdTRuO0TbL2MmW0zKyvaABxWTvWyghPHpS1rykjNn9E1u6mjEAK3bMAWNYZtTJoFibrJFMGSqQ1BABZ59XK8OdSswBeWafWynCmU7MAkgVwyTp7Bw1PRjULIFmAZ1lv2EHjMamaBZAsgGQBHmS954Zl3OdVswCSBZAsgGQBJAsgWQDJAkgWQLIAkgWQLIBkAR5kTf+vtcHc51WzAJIFkCzAs6z33LYek6pZAMkCuGS9YRM9GdUsgGQBvLLO3kRnOjULAGSdWi5/LjULwGSdVy6UCDfrJF80i9YQEJF1RrkCKYLN2t1XbP74Gu7rKzy57lmAJlk7lqtl5tZm7eWrcdqENdzFV/ucOQf32BzL/rVu1suZeYNfs2KJUyW/G67mK3ee/GPsFlnJHi9br89ZcyvW6eodj96cUrFdzyk1hik74QRcoyZJt3bg2cqVLGuvOLW78j2tzoMHTLfgRF/RACQLIFkAyQJIFkCyAJIFkCzAf9fNceNTJxYrAAAAAElFTkSuQmCC",
      messageId = message.Id.ToString()
    };

    await _httpClient.PostAsJsonAsync<SaveImageRequest>("api/image/save", req);
    var savedPicture = await _dbcontext.Pictures.FirstOrDefaultAsync(p => p.BelongsTo == 1);
    Assert.NotNull(savedPicture.NameOfFile);
    Assert.Equal(1, savedPicture.BelongsTo);
  }
}