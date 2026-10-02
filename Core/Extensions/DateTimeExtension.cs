using System.Runtime.InteropServices;

namespace ManageLife.Core
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

        /// <summary>Đổi giờ VN (treo tường, Kind Unspecified) sang UTC.</summary>
        public static DateTime ToUtcFromVnTime(this DateTime vnDateTime)
        {
            var unspecified = DateTime.SpecifyKind(vnDateTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(unspecified, VnTimeZone);
        }
    }
}
