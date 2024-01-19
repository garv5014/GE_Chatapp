using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Chatapp.Shared.Telemetry
{
    public static class DiagnosticConfig
    {
        public static string ServiceName = "ge_chatapp";
        public static Meter Meter = new(ServiceName);
        public static Counter<int> userCount = Meter.CreateCounter<int>("users.count");
        public static ActivitySource Source = new(ServiceName);
    }
}
