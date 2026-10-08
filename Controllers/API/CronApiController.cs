using ManageLife.Core;
using ManageLife.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.API
{
    /// <summary>Endpoint cho cron job (cron-job.org) gọi, xác thực bằng header X-Cron-Secret.</summary>
    [Route("api/cron")]
    [AllowAnonymous]
    [CronSecret]
    public class CronApiController : ApiControllerBase
    {
        private readonly ITodoDailySummaryService _dailySummaryService;

        public CronApiController(ITodoDailySummaryService dailySummaryService)
        {
            _dailySummaryService = dailySummaryService;
        }

        [HttpPost("todo-daily-summary")]
        public async Task<IActionResult> SendTodoDailySummary(CancellationToken ct)
        {
            var rs = await _dailySummaryService.SendDailySummaryAsync(ct);

            if (rs.IsOk())
                return Ok();

            return StatusCode(500, rs.Message);
        }
    }
}
