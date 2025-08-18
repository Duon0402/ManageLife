using ManageLife.Base;
using ManageLife.Data;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers
{
    [Route("Admin")]
    [Route("Admin/[controller]")]
    public class DashboardController : WebAdminControllerBase
    {
        public DashboardController(AppDbContext context, ILogger? logger = null) : base(context, logger)
        {
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
