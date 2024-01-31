using Chatapp.Shared.Simple_Models;

namespace Chatapp.Shared.Interfaces;

public interface IFileAPIService
{
  Task<string> RetrieveImageFromFileApi(string imageId);
  Task PostImageToFileApi(SaveImageRequest imageRequest);
}