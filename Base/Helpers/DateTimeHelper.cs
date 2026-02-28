using System.Runtime.InteropServices;

namespace ManageLife.Base
{
    public static class DateTimeHelper
    {
        public static DateTime Now()
        {
            return DateTime.Now;
        }

        public static DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }

        public static DateTime VNTime()
        {
            string timeZoneId;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                timeZoneId = "SE Asia Standard Time";
            }
            else
            {
                timeZoneId = "Asia/Ho_Chi_Minh";
            }

            TimeZoneInfo vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        }
    }
}