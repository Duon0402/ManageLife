using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITelegramFileService
    {
        Task<Result<FileModel>> SaveTempFileAsync(IFormFile file, string? caption = null);
        Task<Result> UploadToTelegramAsync(string fileId);
        Task<Result<string>> GetFileUrlByFileIdAsync(string fileId);
        Task<Result<FileEntity>> GetFileEntityAsync(string fileId);
        Task<Result<Stream>> DownloadFileStreamAsync(string telegramFileId);
    }
}
