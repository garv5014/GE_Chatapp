using System.Net;
using System.Text.Json;

using Chatapp.Shared.Entities;
using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Services;
using Chatapp.Shared.Simple_Models;

using Moq;
using Moq.Protected;

namespace Chatapp.Tests
{
  public class ChatServiceTests
  {
    private Mock<HttpMessageHandler>? _mockHttpMessageHandler;
    private HttpClient? _mockHttpClient;
    private ChatService? _chatService;

    [SetUp]
    public void Setup()
    {
      _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
      _mockHttpMessageHandler.Protected()
          .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(new HttpResponseMessage
          {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(new List<MessageWithImages> { new MessageWithImages { Message = new Message { MessageText = "Test Message" } } }))
          });

      _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object)
      {
        BaseAddress = new Uri("http://test.com")
      };
      var mockFileService = new Mock<IFileAPIService>();

      mockFileService.Setup(m => m.RetrieveImageFromFileApi(It.IsAny<string>())).ReturnsAsync("");

      _chatService = new ChatService(_mockHttpClient, mockFileService.Object);
    }

    [Test]
    public async Task GetMessagesAsync_ReturnsListOfMessages()
    {
      var result = await _chatService.GetMessagesAsync();

      Assert.IsNotNull(result);
      Assert.That(result, Has.Count.EqualTo(1));
      Assert.That(result[0].Message.MessageText, Is.EqualTo("Test Message"));

      // Verify that the GetAsync method was called only once
      _mockHttpMessageHandler.Protected().Verify(
          "SendAsync",
          Times.Once(),
          ItExpr.Is<HttpRequestMessage>(req =>
              req.Method == HttpMethod.Get
              && req.RequestUri.ToString().EndsWith("api/chat")),
          ItExpr.IsAny<CancellationToken>());
    }
  }
}