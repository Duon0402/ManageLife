using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITelegramFileService
    {
        Task<Result<FileModel>> UploadFileAsync(IFormFile file, string? caption = null);

        Task<Result<string>> GetFileUrlByFileIdAsync(string fileId);
    }
}
