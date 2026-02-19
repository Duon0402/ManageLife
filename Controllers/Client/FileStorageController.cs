using ManageLife.Base;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class FileStorageController : WebClientControllerBase
    {
        private readonly ITelegramFileService _telegramFileService;

        public FileStorageController(ITelegramFileService telegramFileService)
        {
            _telegramFileService = telegramFileService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<Result<FileModel>> Upload(IFormFile file)
        {
            var result = await _telegramFileService.UploadFileAsync(file);
            return result;
        }

        [HttpGet]
        public async Task<Result<string>> GetFileUrl(string fileId)
        {
            var result = await _telegramFileService.GetFileUrlByFileIdAsync(fileId);
            return result;
        }
    }
}
