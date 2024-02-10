using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Chatapp.Shared.Telemetry;

public static class DiagnosticConfig
{
  public static string ServiceName = "ge_chatapp";
  public static Meter Meter = new(ServiceName);
  public static Counter<int> backedUpPhotos = Meter.CreateCounter<int>("backedup.photos");
  public static Counter<int> totalPhotos = Meter.CreateCounter<int>("total.photos");
  public static Counter<int> userCount = Meter.CreateCounter<int>("users.count");
  public static Counter<int> messageCount = Meter.CreateCounter<int>("messages.count");
  public static Counter<int> messageWithImageCount = Meter.CreateCounter<int>("message.with.image");
  public static Counter<int> newMessageFailedCount = Meter.CreateCounter<int>("messages.new.failed.count");
  public static Counter<int> retrieveAllMessagesFailedCount = Meter.CreateCounter<int>("messages.retrieve.failed.count");
  public static ActivitySource Source = new(ServiceName);
}