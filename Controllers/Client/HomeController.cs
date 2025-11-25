using ManageLife.Base;
using ManageLife.Data;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class HomeController : WebClientControllerBase
    {

        public HomeController(AppDbContext context, ILogger? logger = null) : base(context, logger)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
