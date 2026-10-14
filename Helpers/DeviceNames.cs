using System.Globalization;
using System.Text;
using ManageLife.Core;
using Microsoft.AspNetCore.Http;

namespace ManageLife.Helpers
{
    /// <summary>Tên thiết bị gửi kèm phiên đăng nhập: header X-Device-Name (app gửi), không có thì đoán từ User-Agent.</summary>
    public static class DeviceNames
    {
        public static string Describe(HttpRequest? request)
        {
            //NOTE: App gửi tên đã encodeURIComponent (header chỉ nhận ASCII, tên máy có thể có dấu)
            var header = request?.Headers["X-Device-Name"].ToString();
            var deviceName = header.IsNotEmpty() ? Clean(SafeUnescape(header!)) : null;
            if (deviceName.IsNotEmpty()) return deviceName!;

            var userAgent = request?.Headers.UserAgent.ToString() ?? "";
            var browser = userAgent.Contains("Edg/") ? "Edge"
                : userAgent.Contains("Chrome/") ? "Chrome"
                : userAgent.Contains("Firefox/") ? "Firefox"
                : userAgent.Contains("Safari/") ? "Safari"
                : null;
            var os = userAgent.Contains("Windows") ? "Windows"
                : userAgent.Contains("Android") ? "Android"
                : userAgent.Contains("iPhone") || userAgent.Contains("iPad") ? "iOS"
                : userAgent.Contains("Mac OS X") ? "macOS"
                : userAgent.Contains("Linux") ? "Linux"
                : null;
            var name = string.Join(" · ", new[] { browser, os }.Where(x => x != null));
            return name.IsNotEmpty() ? name : "Thiết bị không rõ";
        }

        /// <summary>
        /// Bỏ ký tự điều khiển + ký tự định dạng ẩn (bidi, zero-width) + ngắt dòng/đoạn Unicode.
        /// </summary>
        private static string Clean(string value)
        {
            var visible = value.Where(c => !char.IsControl(c) && CharUnicodeInfo.GetUnicodeCategory(c) is not
                (UnicodeCategory.Format or UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator));
            var cleaned = string.Join(' ', new string(visible.ToArray()).Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (cleaned.Length <= 100) return cleaned;
            var cut = char.IsHighSurrogate(cleaned[99]) ? 99 : 100;
            return cleaned[..cut];
        }

        private static string SafeUnescape(string value)
        {
            try { return Uri.UnescapeDataString(value); }
            catch (UriFormatException) { return value; }
        }
    }
}
