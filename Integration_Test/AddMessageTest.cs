using Chatapp.Shared.Entities;

namespace Integration_Test;

public class AddMessageTest : BaseIntegrationTest
{
  public AddMessageTest(IntegrationTestWebAppFactory factory) : base(factory)
  {
  }

  [Fact]
  public async Task AddMessageTestAsync()
  {
    var message = new Message
    {
      MessageText = "Hello World",
      CreatedAt = DateTime.Now,
      Username = "Test User"
    };

    await _chatService.SendMessageAsync(message);
    var messages = await _chatService.GetMessagesAsync();
    Assert.Equal(1, messages.Count);
    Assert.Equal("Hello World", messages[0].MessageText);
    Assert.Equal("Test User", messages[0].Username);
  }
}
