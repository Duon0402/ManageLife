using Serilog.Core;
using Serilog.Events;

namespace ManageLife.Base
{
    public class VietnamTimeEnricher : ILogEventEnricher
    {
        private static readonly TimeZoneInfo VietnamTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows()
                    ? "SE Asia Standard Time"
                    : "Asia/Ho_Chi_Minh"
            );

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(
                logEvent.Timestamp.UtcDateTime,
                VietnamTimeZone
            );

            logEvent.AddOrUpdateProperty(
                propertyFactory.CreateProperty(
                    "Timestamp",
                    vnTime.ToString("yyyy-MM-dd HH:mm:ss")
                )
            );
        }
    }
}
