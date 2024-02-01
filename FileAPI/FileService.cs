using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Telemetry;

using FileAPI.Options;

using ImageMagick;

namespace Chatapp.Shared.Services;

public class FileService : IFileService
{
  private readonly ILogger _logger;
  private readonly FileAPIOptions _fileAPIOptions;

  public FileService(ILogger<FileService> logger, FileAPIOptions fileAPIOptions)
  {
    _logger = logger;
    _fileAPIOptions = fileAPIOptions;
  }

  public string RetrieveImageFromDrive(string imagePath)
  {
    if (!File.Exists(imagePath))
    {
      throw new FileNotFoundException("The image file was not found.", imagePath);
    }
    Task.Delay(_fileAPIOptions.APIDelayInSeconds * 1000).Wait();

    // Read the file into a byte array
    byte[] imageBytes = File.ReadAllBytes(imagePath);

    // Convert the byte array to a Base64 string
    string base64String = Convert.ToBase64String(imageBytes);

    // Construct the Data URI
    return $"data:image/png;base64,{base64String}";
  }


  public string SaveImageToDrive(string base64Image)
  {
    var NameOfFile = Guid.NewGuid().ToString();
    // save to drive
    _logger.LogInformation($"Saving image {NameOfFile} to drive");
    Task.Delay(_fileAPIOptions.APIDelayInSeconds * 1000).Wait();
    byte[] bytes = Convert.FromBase64String(base64Image);
    string filePath = Path.Combine("/app/images", NameOfFile + ".png");

    File.WriteAllBytes(filePath, bytes); // Write the file to the filesystem

    if (_fileAPIOptions.CompressImages)
    {
      using (var imageCompression = DiagnosticConfig.Source.StartActivity(DiagnosticNames.imageCompression))
      {
        imageCompression?.SetTag(DiagnosticNames.imageCompressionId, NameOfFile);

        var optimizer = new ImageOptimizer();
        _logger.LogInformation($"Compressing image {NameOfFile} to filesystem");
        optimizer.Compress(filePath);
      }
    }

    return NameOfFile;
  }
}