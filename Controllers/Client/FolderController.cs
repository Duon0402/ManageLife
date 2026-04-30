using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class FolderController : WebClientControllerBase
    {
        private readonly IFolderService _folderService;
        private readonly ITelegramFileService _telegramFileService;

        public FolderController(IFolderService folderService, ITelegramFileService telegramFileService)
        {
            _folderService = folderService;
            _telegramFileService = telegramFileService;
        }

        /// <summary>Trang danh sách folder</summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>Trang chi tiết folder (gallery ảnh)</summary>
        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            var result = await _folderService.GetFoldersAsync();
            var folder = result.Data?.FirstOrDefault(f => f.Id == id);
            if (folder == null)
                return RedirectToAction("Index");

            ViewBag.FolderId = folder.Id;
            ViewBag.FolderName = folder.Name;
            return View("Detail");
        }

        /// <summary>API: Lấy tất cả folder</summary>
        [HttpGet]
        public async Task<Result<List<FolderModel>>> GetAll()
        {
            return await _folderService.GetFoldersAsync();
        }

        /// <summary>API: Tạo folder mới</summary>
        [HttpPost]
        public async Task<Result<FolderModel>> Create([FromBody] CreateFolderCommand cmd)
        {
            return await _folderService.CreateFolderAsync(cmd);
        }

        /// <summary>API: Xoá folder</summary>
        [HttpDelete]
        public async Task<Result> Delete(string id)
        {
            return await _folderService.DeleteFolderAsync(id);
        }

        /// <summary>API: Lấy danh sách file trong folder</summary>
        [HttpGet]
        public async Task<Result<List<FolderFileItemModel>>> Files(string id)
        {
            return await _folderService.GetFolderFilesAsync(id);
        }

        /// <summary>API: Upload file và tự động link vào folder</summary>
        [HttpPost]
        public async Task<Result<FileModel>> Upload(string id, IFormFile file)
        {
            // Bước 1: Upload file qua TelegramFileService
            var uploadResult = await _telegramFileService.SaveTempFileAsync(file);
            if (!uploadResult.IsOk())
                return uploadResult;

            // Bước 2: Link file vào folder
            var linkResult = await _folderService.AddFileToFolderAsync(id, uploadResult.Data!.Id);
            if (!linkResult.IsOk())
                return Result.Error<FileModel>(linkResult.Code, linkResult.Message);

            return uploadResult;
        }

        /// <summary>API: Xoá file khỏi folder</summary>
        [HttpDelete]
        public async Task<Result> RemoveFile(string folderId, string fileId)
        {
            return await _folderService.RemoveFileFromFolderAsync(folderId, fileId);
        }
    }
}
