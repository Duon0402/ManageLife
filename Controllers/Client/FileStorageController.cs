using ManageLife.Core;
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
        public async Task<Result<FileModel>> Upload(IFormFile file, CancellationToken ct)
        {
            return await _telegramFileService.SaveTempFileAsync(file, ct: ct);
        }

        [HttpGet]
        public async Task<IActionResult> GetFile(string fileId, CancellationToken ct)
        {
            var entityResult = await _telegramFileService.GetFileEntityAsync(fileId, ct);
            if (!entityResult.IsOk())
            {
                return NotFound(entityResult.Message);
            }

            var entity = entityResult.Data!;

            if (entity.Status == UploadStatus.Completed && !string.IsNullOrEmpty(entity.FileId))
            {
                var etag = $"\"{entity.Id}\"";
                if (Request.Headers.IfNoneMatch == etag)
                    return StatusCode(304);

                var streamResult = await _telegramFileService.DownloadFileStreamAsync(entity.FileId, ct);
                if (streamResult.IsOk())
                {
                    var contentType = entity.FileType ?? "image/jpeg";
                    Response.Headers["Cache-Control"] = "public, max-age=604800, immutable";
                    Response.Headers["ETag"] = etag;
                    return File(streamResult.Data!, contentType, entity.FileName);
                }
            }

            if (!string.IsNullOrEmpty(entity.TempPath))
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), entity.TempPath);
                if (System.IO.File.Exists(fullPath))
                {
                    var contentType = entity.FileType ?? "application/octet-stream";
                    Response.Headers["Cache-Control"] = "public, max-age=3600";
                    return PhysicalFile(fullPath, contentType, entity.FileName);
                }
            }

            return NotFound("File not found locally or on Telegram");
        }

        [HttpGet]
        public async Task<Result<string>> GetFileUrl(string fileId, CancellationToken ct)
        {
            return await _telegramFileService.GetFileUrlByFileIdAsync(fileId, ct);
        }

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<FileModel>>> GetListFiles(CancellationToken ct)
        {
            return await _telegramFileService.GetListFilesAsync(ct);
        }
    }
}
