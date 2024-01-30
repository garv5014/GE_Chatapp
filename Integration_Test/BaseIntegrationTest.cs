using Chatapp.Shared.Interfaces;

using Microsoft.Extensions.DependencyInjection;

namespace Integration_Test;

public abstract class BaseIntegrationTest : IClassFixture<MessageApiWebApplicationFactory>
{
  private readonly IServiceScope _scope;
  protected readonly IChatService _chatService;
  protected BaseIntegrationTest(MessageApiWebApplicationFactory factory)
  {
    _scope = factory.Services.CreateScope();
    _chatService = _scope.ServiceProvider.GetRequiredService<IChatService>();
  }
}