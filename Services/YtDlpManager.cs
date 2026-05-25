using ManageLife.Core;
using ManageLife.Settings;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ManageLife.Services
{
    public class YtDlpManager
    {
        private readonly IAppLogger<YtDlpManager> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _configuredPath;

        private static readonly SemaphoreSlim _lock = new(1, 1);
        private static bool _initialized = false;

        private string _resolvedPath = string.Empty;
        public string ExecutablePath => _resolvedPath;

        private static readonly string ToolsDir = Path.Combine(AppContext.BaseDirectory, "tools");

        private static readonly string ManagedPath = Path.Combine(ToolsDir,
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "yt-dlp.exe" : "yt-dlp");

        private const string YtDlpDownloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/";

        public YtDlpManager(IOptions<VideoDownloaderSettings> options, IHttpClientFactory httpClientFactory, IAppLogger<YtDlpManager> logger)
        {
            _configuredPath = options.Value.YtDlpPath;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _resolvedPath = _configuredPath;
        }

        public async Task EnsureReadyAsync(CancellationToken ct = default)
        {
            if (_initialized) return;

            await _lock.WaitAsync(ct);
            try
            {
                if (_initialized) return;
                await SetupAsync(ct);
                _initialized = true;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task SetupAsync(CancellationToken ct)
        {
            var isCustomPath = !_configuredPath.Equals("yt-dlp", StringComparison.OrdinalIgnoreCase)
                            && !_configuredPath.Equals("yt-dlp.exe", StringComparison.OrdinalIgnoreCase);

            if (isCustomPath)
            {
                _resolvedPath = _configuredPath;
                _logger.Info($"yt-dlp: dùng path tùy chỉnh '{_resolvedPath}'");
                return;
            }

            if (await CanRunAsync(_configuredPath, ct))
            {
                _resolvedPath = _configuredPath;
                _logger.Info("yt-dlp: tìm thấy trong PATH hệ thống");
                await SelfUpdateAsync(_resolvedPath, ct);
                return;
            }

            if (File.Exists(ManagedPath) && await CanRunAsync(ManagedPath, ct))
            {
                _resolvedPath = ManagedPath;
                _logger.Info($"yt-dlp: tìm thấy tại '{ManagedPath}'");
                await SelfUpdateAsync(_resolvedPath, ct);
                return;
            }

            _logger.Info("yt-dlp: chưa cài đặt, đang tải về...");
            await DownloadYtDlpAsync(ct);
        }

        // ──────────────────────────────────────────────────────────────
        // yt-dlp install / update
        // ──────────────────────────────────────────────────────────────

        private async Task DownloadYtDlpAsync(CancellationToken ct)
        {
            Directory.CreateDirectory(ToolsDir);
            var fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "yt-dlp.exe" : "yt-dlp";

            try
            {
                var client = CreateHttpClient();
                using var response = await client.GetAsync(YtDlpDownloadUrl + fileName, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(ct);
                await using var file = File.Create(ManagedPath);
                await stream.CopyToAsync(file, ct);

                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    File.SetUnixFileMode(ManagedPath,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                        UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                        UnixFileMode.OtherRead | UnixFileMode.OtherExecute);

                _resolvedPath = ManagedPath;
                _logger.Info($"yt-dlp: tải về thành công tại '{ManagedPath}'");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "yt-dlp: tải về thất bại");
            }
        }

        private async Task SelfUpdateAsync(string path, CancellationToken ct)
        {
            try
            {
                _logger.Info("yt-dlp: kiểm tra cập nhật...");
                var (output, _) = await RunAsync(path, ["-U"], ct);
                _logger.Info($"yt-dlp update: {output?.Trim() ?? "không có thông tin"}");
            }
            catch (Exception ex)
            {
                _logger.Warning($"yt-dlp: cập nhật thất bại - {ex.Message}");
            }
        }

        private static async Task<bool> CanRunAsync(string path, CancellationToken ct)
        {
            try
            {
                var (output, _) = await RunAsync(path, ["--version"], ct);
                return output.IsNotEmpty();
            }
            catch { return false; }
        }

        private static async Task<(string? output, string? error)> RunAsync(string path, string[] args, CancellationToken ct)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            foreach (var arg in args)
                startInfo.ArgumentList.Add(arg);

            using var process = Process.Start(startInfo);
            if (process == null) return (null, "process null");

            var output = await process.StandardOutput.ReadToEndAsync(ct);
            var error = await process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);

            return (output.IsNotEmpty() ? output : null, error.IsNotEmpty() ? error : null);
        }

        private HttpClient CreateHttpClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "ManageLife/1.0");
            return client;
        }
    }
}
