using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class HomeController : WebClientControllerBase
    {
        private readonly IConfiguration _config;
        private readonly TelegramService _telegramService;

        public HomeController(AppDbContext context, IConfiguration config, ILogger? logger = null) : base(context, logger)
        {
            _config = config;
            _telegramService = new TelegramService(_config);
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<Result> SendMessage([FromBody] string message)
        {
            var rs = await _telegramService.SendMessageAsync(message);
            return rs;
        }
    }
}
