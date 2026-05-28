using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITelegramFileService
    {
        Task<Result<FileModel>> SaveTempFileAsync(IFormFile file, string? caption = null, CancellationToken ct = default);
        Task<Result> UploadToTelegramAsync(string fileId, CancellationToken ct = default);
        Task<Result<string>> GetFileUrlByFileIdAsync(string fileId, CancellationToken ct = default);
        Task<Result<FileEntity>> GetFileEntityAsync(string fileId, CancellationToken ct = default);
        Task<Result<Stream>> DownloadFileStreamAsync(string telegramFileId, CancellationToken ct = default);
        Task<Result<List<FileModel>>> GetListFilesAsync(CancellationToken ct = default);
    }
}
