using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class UtilityController : WebClientControllerBase
    {
        private readonly IUtilityService _service;

        public UtilityController(AppDbContext context, IUtilityService service, ILogger? logger = null) : base(context, logger)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult EmailDailyReport()
        {
            return View("EmailDailyReport");
        }

        [HttpPost]
        public Result<EmailDailyReportModel> GenerateEmailDailyReport()
        {
            var rs = _service.GenerateEmailDailyReport();
            return rs;
        }
    }
}
