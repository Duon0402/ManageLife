using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class UtilityController : WebClientControllerBase
    {
        private readonly IUtilityService _service;

        public UtilityController(IUtilityService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult EmailDailyReport()
        {
            return View("EmailDailyReport");
        }

        [Permission("GenerateEmailDailyReport")]
        [HttpPost]
        public Result<EmailDailyReportModel> GenerateEmailDailyReport([FromBody] GenerateEmailDailyReportRequest request)
        {
            var rs = _service.GenerateEmailDailyReport(request);
            return rs;
        }
    }
}
