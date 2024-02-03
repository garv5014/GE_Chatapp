namespace Chatapp.Shared.Interfaces;

public interface IRedisService
{
  public Task StoreValue(string key, string value);
  public string RetrieveKeyValue(string key);
  public bool KeyExists(string key);
}