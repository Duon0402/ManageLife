using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITelegramFileService
    {
        Task<Result<FileModel>> SaveTempFileAsync(IFormFile file, string? caption = null);
        Task<Result> UploadToTelegramAsync(string fileId);
        Task<Result<string>> GetFileUrlByFileIdAsync(string fileId);
    }
}
