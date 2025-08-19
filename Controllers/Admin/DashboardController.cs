using ManageLife.Base;
using ManageLife.Data;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers
{
    public class DashboardController : WebAdminControllerBase
    {
        public DashboardController(AppDbContext context, ILogger? logger = null) : base(context, logger)
        {
        }

        public override IActionResult Index()
        {
            return base.Index();
        }
    }
}
