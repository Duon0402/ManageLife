using ManageLife.Base;
using ManageLife.Entities;
using ManageLife.Helpers;
using ManageLife.Models;
using Telegram.Bot.Types;

namespace ManageLife.Interfaces
{
    public interface ITelegramFileService
    {
        Task<Result<FileModel>> UploadFileAsync(IFormFile file, string? caption = null);

        Task<Result<string>> GetFileUrlByFileIdAsync(string fileId);
    }
}
