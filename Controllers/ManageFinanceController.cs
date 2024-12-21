using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers
{
    public class ManageFinanceController : Controller
    {
        public ManageFinanceController()
        {

        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
