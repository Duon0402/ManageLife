namespace ManageLife.Settings
{
    public class VideoDownloaderOptions
    {
        public const string Section = "VideoDownloader";

        public string YtDlpPath { get; set; } = "yt-dlp";

        // Đường dẫn file cookies (Netscape format) xuất từ browser.
        public string? CookiesFile { get; set; }

        // Tên browser để tự extract cookies: firefox, chrome, edge, brave
        // Firefox là lựa chọn hoạt động được trên Chrome 127+ (App-Bound Encryption)
        public string? CookiesBrowser { get; set; }
    }
}
