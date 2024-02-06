using Chatapp.Shared;
using Chatapp.Shared.Interfaces;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Testcontainers.PostgreSql;

namespace Integration_Test;

public class FileApiWebApplicationFactory : WebApplicationFactory<FileAPI.Program>, IAsyncLifetime
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

      var fileServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IFileService));
      if (fileServiceDescriptor != null)
      {
        services.Remove(fileServiceDescriptor);
      }

      // moq file service with moq
      var newFileServiceMok = new Mock<IFileService>();
      newFileServiceMok.Setup(x => x.SaveImageToDrive(It.IsAny<string>())).Returns(Guid.NewGuid().ToString());

      services.AddScoped<IFileService>(_ => newFileServiceMok.Object);

      var redisServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRedisService));
      if (redisServiceDescriptor != null)
        services.Remove(redisServiceDescriptor);

      // Mock Redis service with Moq
      var newRedisServiceMok = new Mock<IRedisService>();
      newRedisServiceMok.Setup(rs => rs.KeyExists(It.IsAny<string>())).Returns(true);
      newRedisServiceMok.Setup(rs => rs.RetrieveKeyValue(It.IsAny<string>())).Returns("mocked value");
      newRedisServiceMok.Setup(rs => rs.StoreValue(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

      services.AddScoped<IRedisService>(_ => newRedisServiceMok.Object);

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