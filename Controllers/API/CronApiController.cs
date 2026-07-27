using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ManageLife.Controllers.API
{
    [Route("api/cron")]
    [AllowAnonymous]
    public class CronApiController : ApiControllerBase
    {
        private readonly ITodoReminderService _reminderService;
        private readonly string _webhookSecret;

        public CronApiController(
            ITodoReminderService reminderService,
            IOptions<CronJobOptions> cronSettings)
        {
            _reminderService = reminderService;
            _webhookSecret = cronSettings.Value.WebhookSecret;
        }

        [HttpPost("todo-reminders")]
        public async Task<IActionResult> ProcessTodoReminders(
            [FromHeader(Name = "X-Cron-Secret")] string? secret,
            CancellationToken ct)
        {
            if (!IsValidSecret(secret))
                return Unauthorized();

            var rs = await _reminderService.ProcessPendingRemindersAsync(ct);

            if (rs.IsOk())
                return Ok();

            return StatusCode(500, rs.Message);
        }

        [HttpPost("todo-daily-summary")]
        public async Task<IActionResult> SendDailySummary(
            [FromHeader(Name = "X-Cron-Secret")] string? secret,
            CancellationToken ct)
        {
            if (!IsValidSecret(secret))
                return Unauthorized();

            var rs = await _reminderService.SendDailySummaryAsync(ct);

            if (rs.IsOk())
                return Ok();

            return StatusCode(500, rs.Message);
        }

        private bool IsValidSecret(string? secret)
        {
            if (string.IsNullOrWhiteSpace(_webhookSecret))
                return false;

            return string.Equals(secret, _webhookSecret, StringComparison.Ordinal);
        }
    }
}
