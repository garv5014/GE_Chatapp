using System.Text.Json.Serialization;

using Chatapp.Shared;
using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Services;
using Chatapp.Shared.Telemetry;

using GE_Chatapp.Client.Pages;
using GE_Chatapp.Components;

using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace GE_Chatapp;

//https://www.youtube.com/watch?v=c4AJlZeX2fE
// Configure OpenTelemetry Tracing
public partial class Program
{
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    Uri collector_uri = new Uri(builder?.Configuration["CollectorURL"] ?? throw new Exception("No Collector Menu Found"));
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resourceBuilder =>
        {
          resourceBuilder
              .AddService("Chat_App");
        })
      .WithTracing(tracing =>
      {
        tracing
            .AddAspNetCoreInstrumentation() // Automatic instrumentation for ASP.NET Core
            .AddHttpClientInstrumentation() // Automatic instrumentation for HttpClient
            .AddEntityFrameworkCoreInstrumentation()
            .AddSource(DiagnosticConfig.Source.Name)
            .AddOtlpExporter(options =>
            {
              options.Endpoint = collector_uri; // OTLP exporter endpoint
            });
        // You can add more instrumentation or exporters as needed
      }).WithMetrics(metrics =>
        {
          metrics.AddMeter("Microsoft.AspNetCore.Hosting")
          .AddMeter("Microsoft.AspNetCore.Http")
          .AddMeter(DiagnosticConfig.Meter.Name)
          .AddPrometheusExporter()
          // The rest of your setup code goes here too
          .AddOtlpExporter(options =>
          {
            options.Endpoint = collector_uri;
          });
        });

    builder.Services.AddLogging(l =>
      {
        l.AddOpenTelemetry(o =>
        {
          o.SetResourceBuilder(
              ResourceBuilder.CreateDefault().AddService("Chat_App"))
          .AddOtlpExporter(options =>
          {
            options.Endpoint = collector_uri;
          });
        });
      });

    builder.Services.AddScoped<IFileAPIService, FileApiService>();
    builder.Services
        .AddHttpClient("My.ServerAPI", client => client.BaseAddress = new Uri(builder.Configuration["ApiBaseAddress"] ?? throw new Exception("ApiBaseAddress not found ")));

    builder.Services
        .AddHttpClient("My.FileAPI", client => client.BaseAddress = new Uri(builder.Configuration["FileStoreAPIAddress"] ?? throw new Exception("FileStoreAPIAddress not found ")));

    builder.Services.AddScoped<IChatService, ChatService>(sp => new ChatService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("My.ServerAPI"), sp.GetRequiredService<IFileAPIService>()));

    builder.Services.AddSingleton<SignalREnv>(new SignalREnv()
    {
      IsServer = true,
      chatHubURL = builder.Configuration["ApiBaseAddress"] + "/ws/chathub" ?? throw new Exception("ApiBaseAddress not found ")
    });

    builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("My.FileAPI"));


    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents();


    builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<ChatDbContext>(options =>
    {
      options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
      options.EnableDetailedErrors();
      options.EnableSensitiveDataLogging();
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
      Console.WriteLine("Development mode");
      app.UseWebAssemblyDebugging();
      app.UseSwagger();
      app.UseSwaggerUI();
      app.MapGet("/api/image/{**rest}", async context =>
      {
        var httpClient = new HttpClient();
        var imageUrl = $"http://nginx:80{context.Request.Path.Value}";
        var response = await httpClient.GetAsync(imageUrl);
        if (response.IsSuccessStatusCode)
        {
          var content = await response.Content.ReadAsStreamAsync();
          context.Response.ContentType = response?.Content?.Headers?.ContentType?.ToString();
          await content.CopyToAsync(context.Response.Body);
        }
      });
    }
    else
    {
      Console.WriteLine("Production mode");
      app.UseExceptionHandler("/Error", createScopeForErrors: true);
      // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    }

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(Chat).Assembly);

    app.MapControllers();

    app.MapPrometheusScrapingEndpoint();

    app.Run();
  }
}