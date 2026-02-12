using ManageLife.Base;
using ManageLife.Data;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class DashboardController : WebAdminControllerBase
    {
        public DashboardController(AppDbContext context) : base(context)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
