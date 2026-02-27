using ManageLife.Base;
using ManageLife.Entities;
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
            var result = await _telegramFileService.SaveTempFileAsync(file);
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> GetFile(string fileId)
        {
            var entityResult = await _telegramFileService.GetFileEntityAsync(fileId);
            if (!entityResult.IsOk())
            {
                return NotFound(entityResult.Message);
            }

            var entity = entityResult.Data!;

            // Case 1: Upload is completed and we have a Telegram FileId
            if (entity.Status == UploadStatus.Completed && !string.IsNullOrEmpty(entity.FileId))
            {
                // Check ETag – allow browser to use its cache without hitting the server again
                var etag = $"\"{entity.Id}\"";
                if (Request.Headers.IfNoneMatch == etag)
                    return StatusCode(304);

                var streamResult = await _telegramFileService.DownloadFileStreamAsync(entity.FileId);
                if (streamResult.IsOk())
                {
                    var contentType = entity.FileType ?? "image/jpeg";
                    Response.Headers["Cache-Control"] = "public, max-age=604800, immutable"; // 7 days
                    Response.Headers["ETag"] = etag;
                    return File(streamResult.Data!, contentType, entity.FileName);
                }
            }

            // Case 2: Upload not yet completed or Telegram download failed, fall back to local temp file
            if (!string.IsNullOrEmpty(entity.TempPath))
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), entity.TempPath);
                if (System.IO.File.Exists(fullPath))
                {
                    var contentType = entity.FileType ?? "application/octet-stream";
                    Response.Headers["Cache-Control"] = "public, max-age=3600"; // 1 hour for temp files
                    return PhysicalFile(fullPath, contentType, entity.FileName);
                }
            }

            return NotFound("File not found locally or on Telegram");
        }

        [HttpGet]
        public async Task<Result<string>> GetFileUrl(string fileId)
        {
            var result = await _telegramFileService.GetFileUrlByFileIdAsync(fileId);
            return result;
        }
    }
}
