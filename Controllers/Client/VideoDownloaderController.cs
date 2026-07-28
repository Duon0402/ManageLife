using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Core.Models;
using ManageLife.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class VideoDownloaderController : WebClientControllerBase
    {
        private readonly IVideoDownloaderService _videoDownloaderService;
        private readonly ISettingContext _settingContext;

        public VideoDownloaderController(IVideoDownloaderService videoDownloaderService, ISettingContext settingContext)
        {
            _videoDownloaderService = videoDownloaderService;
            _settingContext = settingContext;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _settingContext.GetBoolAsync(SettingKeys.Feature.EnableVideoDownloader, true))
                return NotFound();
            return View();
        }

        [HttpPost]
        public async Task<Result<VideoInfo>> GetVideoInfo([FromBody] VideoInfoRequest request, CancellationToken ct)
        {
            return await _videoDownloaderService.GetVideoInfoAsync(request.Url, ct);
        }

        [HttpPost]
        public async Task<IActionResult> Download([FromBody] VideoDownloadRequest request, CancellationToken ct)
        {
            if (request.OriginalUrl.IsEmpty())
                return BadRequest("URL không hợp lệ");

            var result = await _videoDownloaderService.DownloadVideoStreamAsync(request.OriginalUrl, ct);
            if (!result.IsOk())
                return BadRequest(result.Message);

            var fileName = request.FileName.IsEmpty()
                ? "video"
                : request.FileName.Replace(" ", "_");

            return File(result.Data, "video/mp4", $"{fileName}.mp4");
        }
    }

    public class VideoInfoRequest
    {
        public string Url { get; set; } = string.Empty;
    }

    public class VideoDownloadRequest
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
