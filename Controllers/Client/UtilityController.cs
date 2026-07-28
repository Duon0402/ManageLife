using ManageLife.Commons;
using ManageLife.Contexts;
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
        private readonly ISettingContext _settingContext;

        public UtilityController(IUtilityService service, ISettingContext settingContext)
        {
            _service = service;
            _settingContext = settingContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> EmailDailyReport()
        {
            if (!await _settingContext.GetBoolAsync(SettingKeys.Feature.EnableEmailDailyReport, true))
                return NotFound();
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
