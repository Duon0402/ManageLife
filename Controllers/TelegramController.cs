using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace ManageLife.Controllers
{
	public class TelegramController : WebControllerBase
	{
		private readonly TelegramService _service;

		public TelegramController(AppDbContext context, IConfiguration config, ILogger? logger = null) : base(context, logger)
		{
			_service = new TelegramService(config);
		}

		public IActionResult Index()
		{
			return View();
		}

	}
}
