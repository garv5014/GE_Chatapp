using Chatapp.Shared;
using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Services;
using Chatapp.Shared.Simple_Models;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Testcontainers.PostgreSql;
namespace Integration_Test;

//https://www.youtube.com/watch?v=tj5ZCtvgXKY
public class MessageApiWebApplicationFactory : WebApplicationFactory<GE_Chatapp.Program>, IAsyncLifetime
{
  private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
    .WithImage("postgres:latest")
    .WithDatabase("test_db")
    .WithUsername("postgres")
    .WithResourceMapping(new DirectoryInfo(FindProjectRootByMarker() + "/chatdb/initscripts"), "/docker-entrypoint-initdb.d")
    .WithBindMount(FindProjectRootByMarker() + "/chatdb/initscripts", "/docker-entrypoint-initdb.d")
    .WithPassword("postgres")
    .Build();

  public async Task InitializeAsync()
  {
    await _dbContainer.StartAsync();
  }

  public new async Task DisposeAsync()
  {
    await _dbContainer.StopAsync();
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureTestServices(services =>
    {
      var clientDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(HttpClient));
      if (clientDescriptor != null)
      {
        services.Remove(clientDescriptor);
      }

      // Register HttpClient to be provided from the factory's CreateClient method
      services.AddScoped(sp =>
      {
        // Create an HttpClient using the in-memory test server
        var client = this.CreateClient();
        return client;
      });

      // Register your ChatService to use the HttpClient from DI
      services.AddScoped<IChatService, ChatService>();

      var fileServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IFileAPIService));
      if (fileServiceDescriptor != null)
      {
        services.Remove(fileServiceDescriptor);
      }

      // moq file service with moq
      var newFileServiceMok = new Mock<IFileAPIService>();
      newFileServiceMok.Setup(x => x.PostImageToFileApi(It.IsAny<SaveImageRequest>()));

      services.AddScoped<IFileAPIService>(_ => newFileServiceMok.Object);

      var descriptor = services.SingleOrDefault(d =>
          d.ServiceType == typeof(DbContextOptions<ChatDbContext>)
        );
      if (descriptor != null)
      {
        services.Remove(descriptor);
      }

      services.AddDbContext<ChatDbContext>(options =>
      {
        options.UseNpgsql(_dbContainer.GetConnectionString());
      });
    });


  }
  public static string FindProjectRootByMarker()
  {
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory != null && !directory.GetFiles("*.sln").Any())
    {
      directory = directory.Parent;
    }

    return directory?.FullName ?? throw new Exception("Project root could not be located.");
  }
}