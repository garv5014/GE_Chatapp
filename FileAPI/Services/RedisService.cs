using Chatapp.Shared.Interfaces;

using StackExchange.Redis;

namespace FileAPI.Services;

public class RedisService : IRedisService
{
  readonly ConnectionMultiplexer _redis;
  readonly IDatabase _db;

  public RedisService()
  {
    _redis = ConnectionMultiplexer.Connect("chatappcache:6379");
    _db = _redis.GetDatabase();
  }

  public bool KeyExists(string key)
  {
    return _db.KeyExists(key);
  }

  public string RetrieveKeyValue(string key)
  {
    return _db.StringGet(key).ToString();
  }

  public async Task StoreValue(string key, string value)
  {
    await _db.StringSetAsync(key, value);
  }
}