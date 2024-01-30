using Chatapp.Shared;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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