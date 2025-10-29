using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class TelegramController : WebClientControllerBase
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
