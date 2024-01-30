using Chatapp.Shared.Entities;
using Chatapp.Shared.Simple_Models;

namespace Integration_Test;

public class AddMessageTest : BaseIntegrationTest
{
  public AddMessageTest(MessageApiWebApplicationFactory factory) : base(factory)
  {
  }

  [Fact]
  public async Task AddMessageTestAsync()
  {
    var message = new MessageWithImages
    {
      Message = new Message
      {
        MessageText = "Hello World",
        CreatedAt = DateTime.Now,
        Username = "Test User"
      }
    };

    await _chatService.SendMessageAsync(message);
    var messages = await _chatService.GetMessagesAsync();
    Assert.Single(messages);
    Assert.Equal("Hello World", messages[0].Message.MessageText);
    Assert.Equal("Test User", messages[0].Message.Username);
  }
}