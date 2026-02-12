using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class TelegramController : WebClientControllerBase
    {
        private readonly ITelegramService _service;

        public TelegramController(AppDbContext context, ITelegramService service) : base(context)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

    }
}
