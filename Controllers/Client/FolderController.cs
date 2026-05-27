using ManageLife.Core;
using ManageLife.Core.Attributes;
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

        [AccessPagePermission]
        public IActionResult Index()
        {
            return View();
        }

        [AccessPagePermission]
        [HttpGet]
        public async Task<IActionResult> Detail(string id, CancellationToken ct)
        {
            var result = await _folderService.GetFoldersAsync(ct);
            var folder = result.Data?.FirstOrDefault(f => f.Id == id);
            if (folder == null)
                return RedirectToAction("Index");

            ViewBag.FolderId = folder.Id;
            ViewBag.FolderName = folder.Name;
            return View("Detail");
        }

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<FolderModel>>> GetAll(CancellationToken ct)
        {
            return await _folderService.GetFoldersAsync(ct);
        }

        [InsertPermission]
        [HttpPost]
        public async Task<Result<FolderModel>> Create([FromBody] CreateFolderCommand cmd, CancellationToken ct)
        {
            return await _folderService.CreateFolderAsync(cmd, ct);
        }

        [DeletePermission]
        [HttpDelete]
        public async Task<Result> Delete(string id, CancellationToken ct)
        {
            return await _folderService.DeleteFolderAsync(id, ct);
        }

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<FolderFileItemModel>>> Files(string id, CancellationToken ct)
        {
            return await _folderService.GetFolderFilesAsync(id, ct);
        }

        [InsertPermission]
        [HttpPost]
        public async Task<Result<FileModel>> Upload(string id, IFormFile file, CancellationToken ct)
        {
            var uploadResult = await _telegramFileService.SaveTempFileAsync(file, ct: ct);
            if (!uploadResult.IsOk())
                return uploadResult;

            var linkResult = await _folderService.AddFileToFolderAsync(id, uploadResult.Data!.Id, ct);
            if (!linkResult.IsOk())
                return Result.Error<FileModel>(linkResult.Code, linkResult.Message);

            return uploadResult;
        }

        [DeletePermission]
        [HttpDelete]
        public async Task<Result> RemoveFile(string folderId, string fileId, CancellationToken ct)
        {
            return await _folderService.RemoveFileFromFolderAsync(folderId, fileId, ct);
        }
    }
}
