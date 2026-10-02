using ManageLife.Core;
using ManageLife.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.API
{
    [Route("api/cron")]
    [AllowAnonymous]
    [CronSecret]
    public class CronApiController : ApiControllerBase
    {
        private readonly ITodoReminderService _reminderService;

        public CronApiController(ITodoReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        [HttpPost("todo-reminders")]
        public async Task<IActionResult> ProcessTodoReminders(CancellationToken ct)
        {
            var rs = await _reminderService.ProcessPendingRemindersAsync(ct);

            if (rs.IsOk())
                return Ok();

            return StatusCode(500, rs.Message);
        }

        [HttpPost("todo-daily-summary")]
        public async Task<IActionResult> SendDailySummary(CancellationToken ct)
        {
            var rs = await _reminderService.SendDailySummaryAsync(ct);

            if (rs.IsOk())
                return Ok();

            return StatusCode(500, rs.Message);
        }
    }
}
