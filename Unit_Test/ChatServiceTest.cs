using Chatapp.Shared.Entities;
using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Simple_Models;

using Moq;

namespace Chatapp.Tests
{
  public class ChatServiceTests
  {

    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public async Task GetMessagesAsync_ReturnsListOfMessages()
    {
      var chatMock = new Mock<IChatService>();

      chatMock.Setup(x => x.GetMessagesAsync()).ReturnsAsync(new List<MessageWithImages>
      {
        new MessageWithImages
        {
          Message = new Message
          {
            Id = 1,
            MessageText = "Test Message",
            Username = "Test User",
            CreatedAt = DateTime.Now
          },
          Images = new List<string>()
        }
      });

      var result = await chatMock.Object.GetMessagesAsync();

      Assert.IsNotNull(result);
      Assert.That(result, Has.Count.EqualTo(1));
      Console.WriteLine($"here is the result {result[0].Message.Id}");
      Assert.That(result[0].Message.MessageText, Is.EqualTo("Test Message"));
    }
  }
}