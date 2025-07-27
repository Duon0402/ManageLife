using ManageLife.Base;
using ManageLife.Models;
using ManageLife.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ManageLife.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IConfiguration _config;
		private readonly TelegramService _telegramService;

		public HomeController(ILogger<HomeController> logger, IConfiguration config)
		{
			_logger = logger;
			_config = config;
			_telegramService = new TelegramService(_config);
		}

		public IActionResult Index()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		[HttpPost]
		public async Task<Result> SendMessage([FromBody] string message)
		{
			var rs = await this._telegramService.SendMessageAsync(message);
			return rs;
		}
	}
}
