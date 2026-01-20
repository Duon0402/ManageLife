using System.Runtime.InteropServices;

namespace ManageLife.Base
{
    public static class DateTimeExtension
    {
        private static readonly TimeZoneInfo VnTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "SE Asia Standard Time"
                    : "Asia/Ho_Chi_Minh"
            );

        public static DateTime ToVnTimeFromUtc(this DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "DateTime must be UTC when calling ToVnTimeFromUtc",
                    nameof(utcDateTime)
                );
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VnTimeZone);
        }
    }
}
