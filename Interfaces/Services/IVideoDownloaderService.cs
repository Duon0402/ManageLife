using ManageLife.Core;
using ManageLife.Core.Models;

namespace ManageLife.Interfaces
{
    public interface IVideoDownloaderService
    {
        Task<Result<VideoInfo>> GetVideoInfoAsync(string url, CancellationToken ct = default);
        Task<Result<Stream>> DownloadVideoStreamAsync(string url, CancellationToken ct = default);
    }
}
