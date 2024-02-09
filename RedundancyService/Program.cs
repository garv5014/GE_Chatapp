// See https://aka.ms/new-console-template for more information
using Chatapp.Shared;
using Chatapp.Shared.Entities;

using Microsoft.EntityFrameworkCore;

string? listOfFileServiceURLs = Environment.GetEnvironmentVariable("FILE_SERIVCE_NAMES");

int sleepInterval = int.Parse(Environment.GetEnvironmentVariable("SLEEP_INTERVAL"));
if (listOfFileServiceURLs == null)
{
  throw new Exception("Requires a list of serviceURLS");
}

string? connectionString = Environment.GetEnvironmentVariable("ConnectionString");

var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();
optionsBuilder.UseNpgsql(connectionString);
optionsBuilder.EnableDetailedErrors();
optionsBuilder.EnableSensitiveDataLogging();

var db = new ChatDbContext(options: optionsBuilder.Options);
List<string> listOfServices = new List<string>();
listOfServices = listOfFileServiceURLs.Split(',').ToList();
Console.WriteLine(listOfServices.Count);
Console.WriteLine(listOfServices.First());


while (true)
{
  Task.Delay(sleepInterval * 1000).Wait();
  //get list of non-replicated pictures
  List<PictureLookup> uniquePictures = await db.PictureLookups
    .GroupBy(e => e.PictureId)
    .Where(g => g.Count() == 1)
    .Select(g => g.First())
    .ToListAsync();
  Console.WriteLine(uniquePictures.Count);

  // for each picture send to and image api /replicate endpoint that is not its owner for replication
  foreach (var image in uniquePictures)
  {
    Random random = new Random();

    var filteredList = listOfServices.Where(e => e != image.MachineName).ToList();
    int randomServiceIndex = random.Next(0, filteredList.Count);

    HttpClient client = new HttpClient()
    {
      BaseAddress = new Uri($"http://{filteredList[randomServiceIndex]}:8080")
    };
    Console.WriteLine(client.BaseAddress);
    var res = await client.PostAsync($"/api/image/replicate/{image.PictureId}", null);
    Console.WriteLine(res.StatusCode);
  }

  Console.WriteLine("Hello, World!");
}