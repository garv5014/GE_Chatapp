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
  /// Save a copy of the image to the drive with the given name
  /// </summary>
  /// <param name="base64Image"></param>
  /// <param name="name"></param>
  bool SaveCopyToDrive(string base64Image, string name);
  /// <summary>
  /// Take in image
  /// </summary>
  /// <param name="imageId"></param>
  /// <returns></returns>
  string RetrieveImageFromDrive(string imagePath);
}