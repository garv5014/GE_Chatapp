using System.Text.Json.Serialization;

using Chatapp.Shared;
using Chatapp.Shared.Telemetry;

using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


Uri collector_uri = new Uri(builder?.Configuration["CollectorURL"] ?? throw new Exception("No Collector Menu Found"));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder =>
    {
      resourceBuilder
          .AddService("File_Sevice");
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
        ResourceBuilder.CreateDefault().AddService("File_Sevice"))
    .AddOtlpExporter(options =>
    {
      options.Endpoint = collector_uri;
    });
  });
});

builder.Services.AddControllers().AddJsonOptions(x =>
            x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

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
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
