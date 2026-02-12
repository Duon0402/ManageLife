using ManageLife.Base;
using ManageLife.Data;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class HomeController : WebClientControllerBase
    {

        public HomeController(AppDbContext context) : base(context)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
