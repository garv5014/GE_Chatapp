using Chatapp.Shared.Interfaces;
using StackExchange.Redis;

namespace FileAPI.Services;

public class RedisService : IRedisService
{
  readonly ConnectionMultiplexer _redis;
  readonly IDatabase _db;

  public RedisService()
  {
    _redis = ConnectionMultiplexer.Connect("cache:6379");
    _db = _redis.GetDatabase();
  }

  public Task RetreiveKeyValue(string key)
  {
    throw new NotImplementedException();
  }

  public Task StoreValue(string key, string value)
  {
    throw new NotImplementedException();
  }
}