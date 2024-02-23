using System.Text.Json;

using Chatapp.Shared.Simple_Models;

namespace GE_Chatapp.Client;

public class MessageWithImagesComparer : IComparer<MessageWithImages>
{
  public int Compare(MessageWithImages? x, MessageWithImages? y)
  {
    if (x == null) return y == null ? 0 : -1;
    if (y == null) return 1;

    var xClock = x?.Message.VectorDict != null ? JsonSerializer.Deserialize<Dictionary<string, int>>(x.Message.VectorDict) ?? new Dictionary<string, int>() : new Dictionary<string, int>();
    var yClock = y?.Message.VectorDict != null ? JsonSerializer.Deserialize<Dictionary<string, int>>(y.Message.VectorDict) ?? new Dictionary<string, int>() : new Dictionary<string, int>();

    bool isLess = false;
    bool isGreater = false;

    foreach (var key in xClock.Keys.Union(yClock.Keys))
    {
      int value1 = xClock.TryGetValue(key, out var xVal) ? xVal : 0;
      int value2 = yClock.TryGetValue(key, out var yVal) ? yVal : 0;

      if (value1 <= value2) isLess = true;
      if (value1 > value2) isGreater = true;

      if (isLess && isGreater) return 0; // Concurrent
    }

    if (isLess) return -1;
    if (isGreater) return 1;
    return 0;
  }
}
