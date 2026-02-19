using ManageLife.Base;
using ManageLife.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class TelegramController : WebClientControllerBase
    {
        private readonly ITelegramService _service;

        public TelegramController(ITelegramService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

    }
}
