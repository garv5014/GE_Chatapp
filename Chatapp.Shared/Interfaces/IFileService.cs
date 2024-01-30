namespace Chatapp.Shared.Interfaces;

public interface IFileService
{
  /// <summary>
  /// save the image to the drive and return the name of the image saved.
  /// No name or empty string means image saving failed.
  /// </summary>
  /// <param name="base64Image"></param>
  /// <returns></returns>
  string SaveImageToDrive(string base64Image);
  /// <summary>
  /// Take in image
  /// </summary>
  /// <param name="imageId"></param>
  /// <returns></returns>
  string RetrieveImageFromDrive(string imagePath);
}
