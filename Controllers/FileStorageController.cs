using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Models;
using ManageLife.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers
{
	[Route("filestorage")]
	public class FileStorageController : WebControllerBase
	{
		private readonly TelegramFileService _telegramFileService;

		public FileStorageController(AppDbContext context, IConfiguration config, ILogger? logger = null) : base(context, logger)
		{
			_telegramFileService = new TelegramFileService(context, config);
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost("upload")]
		public async Task<Result<FileModel>> Upload(IFormFile file)
		{
			var result = await _telegramFileService.UploadFileAsync(file);
			return result;
		}

		[HttpGet("get-file-url")]
		public async Task<Result<string>> GetFileUrl(string fileId)
		{
			var result = await _telegramFileService.GetFileUrlByFileIdAsync(fileId);
			return result;
		}
	}
}
