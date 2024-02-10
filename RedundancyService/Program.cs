// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Diagnostics.Metrics;

using Chatapp.Shared;
using Chatapp.Shared.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

string? listOfFileServiceURLs = Environment.GetEnvironmentVariable("FILE_SERIVCE_NAMES");

string? collectorUri = Environment.GetEnvironmentVariable("COLLECTOR_URL");
if (string.IsNullOrEmpty(collectorUri))
{
  throw new Exception("CollectorURL environment variable is not set.");
}

int sleepInterval = int.Parse(Environment.GetEnvironmentVariable("SLEEP_INTERVAL"));
if (listOfFileServiceURLs == null)
{
  throw new Exception("Requires a list of serviceURLS");
}
var resourceBuilder = ResourceBuilder.CreateDefault().AddService("ChatAppConsole");

using var loggerFactory = LoggerFactory.Create(builder =>
{
  builder.AddOpenTelemetry(options =>
  {
    options.SetResourceBuilder(resourceBuilder)
           .AddOtlpExporter(otlpOptions =>
           {
             otlpOptions.Endpoint = new Uri(collectorUri);
           });
    // Additional configuration as needed
  });
});

var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("Start redundancy");

try
{


  string? connectionString = Environment.GetEnvironmentVariable("ConnectionString");

  var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();
  optionsBuilder.UseNpgsql(connectionString);
  optionsBuilder.EnableDetailedErrors();
  optionsBuilder.EnableSensitiveDataLogging();

  var db = new ChatDbContext(options: optionsBuilder.Options);
  List<string> listOfServices = new List<string>();
  listOfServices = listOfFileServiceURLs.Split(',').ToList();


  using var tracerProvider = Sdk.CreateTracerProviderBuilder()
      .SetResourceBuilder(resourceBuilder)
      .AddEntityFrameworkCoreInstrumentation()
      .AddOtlpExporter(options =>
      {
        options.Endpoint = new Uri(collectorUri);
      })
      .Build();

  using var meterProvider = Sdk.CreateMeterProviderBuilder()
      .SetResourceBuilder(resourceBuilder)
       .AddMeter("RedundancyService.Meter")
      .AddOtlpExporter(options =>
      {
        options.Endpoint = new Uri(collectorUri);
      })
      .Build();


  var meter = new Meter("RedundancyService.Meter");

  int unreplicatedPictures = 0;

  var gauge = meter.CreateObservableGauge("unreplicated.pictures", () =>
  {
    return new Measurement<double>(unreplicatedPictures);
  });

  var activitySource = new ActivitySource("RedundancyService");

  while (true)
  {
    Task.Delay(sleepInterval * 1000).Wait();
    logger.LogInformation("Starting redundancy job");
    //get list of non-replicated pictures
    List<PictureLookup> uniquePictures = await db.PictureLookups
      .GroupBy(e => e.PictureId)
      .Where(g => g.Count() == 1)
      .Select(g => g.First())
      .ToListAsync();
    Console.WriteLine(uniquePictures.Count);
    unreplicatedPictures = uniquePictures.Count;

    // for each picture send to and image api /replicate endpoint that is not its owner for replication
    foreach (var image in uniquePictures)
    {
      using var act = activitySource.StartActivity("ReplicateImage");
      Random random = new Random();

      var filteredList = listOfServices.Where(e => e != image.MachineName).ToList();
      int randomServiceIndex = random.Next(0, filteredList.Count);

      HttpClient client = new HttpClient()
      {
        BaseAddress = new Uri($"http://{filteredList[randomServiceIndex]}:8080")
      };
      var res = await client.PostAsync($"/api/image/replicate/{image.PictureId}", null);

    }
    logger.LogInformation("Redundancy job completed");
  }
}
catch (Exception)
{
  logger.LogError("In the redundancy service check internal logs for more information");
  throw;
}