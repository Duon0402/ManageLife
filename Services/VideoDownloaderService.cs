using ManageLife.Core;
using ManageLife.Core.Models;
using ManageLife.Contexts;
using ManageLife.Interfaces;
using ManageLife.Settings;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace ManageLife.Services
{
    public class VideoDownloaderService : ServiceBase<VideoDownloaderService>, IVideoDownloaderService
    {
        private const string CODE_YT_DLP_NO_INFO = "08";
        private const string CODE_YT_DLP_PARSE_FAIL = "09";
        private const string CODE_YT_DLP_DOWNLOAD_FAIL = "11";

        private readonly YtDlpManager _ytDlpManager;
        private readonly string? _cookiesFile;
        private readonly string? _cookiesBrowser;

        public VideoDownloaderService(YtDlpManager ytDlpManager, IOptions<VideoDownloaderSettings> options, IAppLogger<VideoDownloaderService> logger, IUserContext userContext) : base(logger, userContext)
        {
            _ytDlpManager = ytDlpManager;
            _cookiesFile = options.Value.CookiesFile;
            _cookiesBrowser = options.Value.CookiesBrowser;
        }

        public async Task<Result<VideoInfo>> GetVideoInfoAsync(string url, CancellationToken ct = default)
        {
            try
            {
                if (url.IsEmpty())
                    return Result.Error<VideoInfo>(Result.DATA_INVALID.Code, "URL không được để trống");

                await _ytDlpManager.EnsureReadyAsync(ct);

                var args = BuildArgs(["--dump-json", "--no-warnings", "--no-playlist"], url);
                var (json, error) = await RunYtDlpAsync(args, ct);

                // Lỗi truy cập cookies browser → thử lại không dùng cookies (video công khai)
                if (json.IsEmpty() && IsCookieAccessError(error))
                {
                    _logger.Warning($"Không lấy được cookies browser, thử lại không dùng cookies: {error?.Split('\n')[0]}");
                    var argsFallback = BuildArgs(["--dump-json", "--no-warnings", "--no-playlist"], url, includeCookies: false);
                    (json, error) = await RunYtDlpAsync(argsFallback, ct);
                }

                if (json.IsEmpty())
                    return Result.Error<VideoInfo>(CODE_YT_DLP_NO_INFO, $"yt-dlp không lấy được thông tin: {error}");

                var info = ParseVideoJson(json, url);
                if (info == null)
                    return Result.Error<VideoInfo>(CODE_YT_DLP_PARSE_FAIL, "Không thể đọc thông tin video từ yt-dlp");

                return Result.Ok(info);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetVideoInfoAsync error");
                return Result.Exception<VideoInfo>("Lỗi hệ thống khi lấy thông tin video", ex);
            }
        }

        public async Task<Result<Stream>> DownloadVideoStreamAsync(string url, CancellationToken ct = default)
        {
            try
            {
                if (url.IsEmpty())
                    return Result.Error<Stream>(Result.DATA_INVALID.Code, "URL không hợp lệ");

                await _ytDlpManager.EnsureReadyAsync(ct);

                var tempPath = Path.Combine(Path.GetTempPath(), $"vdl_{Guid.NewGuid():N}.mp4");

                var (success, error) = await TryDownloadToFileAsync(url, tempPath, includeCookies: true, ct);

                if (!success && IsCookieAccessError(error))
                {
                    _logger.Warning($"Không lấy được cookies, thử lại không dùng cookies: {error?.Split('\n')[0]}");
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                    tempPath = Path.Combine(Path.GetTempPath(), $"vdl_{Guid.NewGuid():N}.mp4");
                    (success, error) = await TryDownloadToFileAsync(url, tempPath, includeCookies: false, ct);
                }

                if (!success)
                {
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                    return Result.Error<Stream>(CODE_YT_DLP_DOWNLOAD_FAIL, $"yt-dlp tải về thất bại: {error}");
                }

                // DeleteOnClose tự xóa temp file sau khi client nhận xong
                return Result.Ok<Stream>(new FileStream(tempPath, FileMode.Open, FileAccess.Read,
                    FileShare.None, 81920, FileOptions.DeleteOnClose));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DownloadVideoStreamAsync error");
                return Result.Exception<Stream>("Lỗi khi tải video", ex);
            }
        }

        private async Task<(bool success, string? error)> TryDownloadToFileAsync(string url, string outputPath, bool includeCookies, CancellationToken ct)
        {
            var args = BuildArgs(["-o", outputPath, "--no-warnings", "--no-playlist", "--merge-output-format", "mp4"], url, includeCookies);
            var (_, error) = await RunYtDlpAsync(args, ct);
            var success = File.Exists(outputPath) && new FileInfo(outputPath).Length > 0;
            return (success, error);
        }

        // ──────────────────────────────────────────────────────────────
        // Private helpers
        // ──────────────────────────────────────────────────────────────

        private string[] BuildArgs(string[] baseArgs, string url, bool includeCookies = true)
        {
            var args = new List<string>(baseArgs);

            if (includeCookies)
            {
                if (_cookiesFile.IsNotEmpty() && File.Exists(_cookiesFile))
                {
                    args.Add("--cookies");
                    args.Add(_cookiesFile);
                }
                else if (_cookiesBrowser.IsNotEmpty())
                {
                    args.Add("--cookies-from-browser");
                    args.Add(_cookiesBrowser);
                }
            }

            args.Add(url);
            return [.. args];
        }

        private static bool IsCookieAccessError(string? error)
            => error.IsNotEmpty() && error.Contains("cookie", StringComparison.OrdinalIgnoreCase);

        private async Task<(string? output, string? error)> RunYtDlpAsync(string[] args, CancellationToken ct)
        {
            var process = StartYtDlpProcess(args);
            if (process == null) return (null, "Không tìm thấy yt-dlp");

            var output = await process.StandardOutput.ReadToEndAsync(ct);
            var error = await process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);

            return (output.IsNotEmpty() ? output : null, error);
        }

        private Process? StartYtDlpProcess(string[] args)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _ytDlpManager.ExecutablePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            foreach (var arg in args)
                startInfo.ArgumentList.Add(arg);

            try
            {
                return Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Không thể khởi động yt-dlp tại '{_ytDlpManager.ExecutablePath}'");
                return null;
            }
        }

        private static VideoInfo? ParseVideoJson(string json, string originalUrl)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var title = GetString(root, "title") ?? GetString(root, "description") ?? string.Empty;
                var uploader = GetString(root, "uploader") ?? GetString(root, "channel") ?? string.Empty;
                var thumbnail = GetString(root, "thumbnail") ?? string.Empty;
                var duration = root.TryGetProperty("duration", out var d) && d.TryGetDouble(out var sec)
                    ? (int)sec : 0;

                var videoUrl = GetString(root, "url") ?? string.Empty;
                var musicUrl = ExtractMusicUrl(root);

                return new VideoInfo
                {
                    OriginalUrl = originalUrl,
                    Title = title,
                    AuthorNickname = uploader,
                    ThumbnailUrl = thumbnail,
                    VideoUrl = videoUrl,
                    MusicUrl = musicUrl,
                    MusicTitle = string.Empty,
                    Duration = duration,
                };
            }
            catch { return null; }
        }

        private static string ExtractMusicUrl(JsonElement root)
        {
            if (!root.TryGetProperty("formats", out var formats)) return string.Empty;

            foreach (var fmt in formats.EnumerateArray())
            {
                var vcodec = GetString(fmt, "vcodec") ?? string.Empty;
                var acodec = GetString(fmt, "acodec") ?? string.Empty;
                if (vcodec == "none" && acodec.IsNotEmpty())
                    return GetString(fmt, "url") ?? string.Empty;
            }

            return string.Empty;
        }

        private static string? GetString(JsonElement el, string key)
            => el.TryGetProperty(key, out var prop) ? prop.GetString() : null;
    }
}
