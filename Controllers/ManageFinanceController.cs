using ManageLife.Base;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers
{
    public class ManageFinanceController : WebControllerBase
    {
        public ManageFinanceController(ILogger logger) : base(logger)
        {
        }

        public IActionResult Index()
        {
            return View();
        }


    }
}
