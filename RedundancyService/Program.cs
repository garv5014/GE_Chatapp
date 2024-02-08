// See https://aka.ms/new-console-template for more information
string listOfFileServiceURLs = Environment.GetEnvironmentVariable("FILE_SERIVCE_NAMES");

int sleepInterval = int.Parse(Environment.GetEnvironmentVariable("SLEEP_INTERVAL"));
if (listOfFileServiceURLs == null)
{
  throw new Exception("Requires a list of serviceURLS");
}

string connectionString = Environment.GetEnvironmentVariable("ConnectionString");

//make dbcontext

List<string> listOfServices = new List<string>();
listOfServices = listOfFileServiceURLs.Split(',').ToList();

while (true)
{
  Task.Delay(sleepInterval * 1000).Wait();
  //get list of non-replicated pictures
  // for each picture send to and image api /replicate endpoint that is not its owner for replication
  Console.WriteLine("Hello, World!");
}
