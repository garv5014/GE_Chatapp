namespace Chatapp.Shared.Interfaces;

public interface IRedisService
{
  public Task StoreValue(string key, string value);
  public Task RetreiveKeyValue(string key);
}