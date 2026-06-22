using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Models.Pomodoro;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class PomodoroController : WebClientControllerBase
    {
        private readonly IPomodoroService _service;

        public PomodoroController(IPomodoroService service)
        {
            _service = service;
        }

        [AccessPagePermission]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _service.GetSettingsAsync(ct);
            var model = result.Data ?? new PomodoroSettingModel { FocusMinutes = 25, ShortBreakMinutes = 5, LongBreakMinutes = 15 };
            return View(model);
        }

        [HttpGet]
        [ViewPermission]
        public async Task<Result<PomodoroSettingModel>> GetSettings(CancellationToken ct)
            => await _service.GetSettingsAsync(ct);

        [HttpGet]
        [ViewPermission]
        public async Task<Result<PomodoroHistoryModel>> GetHistory([FromQuery] int days, CancellationToken ct)
            => await _service.GetHistoryAsync(days > 0 ? days : 7, ct);

        [HttpPost]
        [InsertPermission]
        public async Task<Result> SaveSession([FromBody] SavePomodoroSessionRequest request, CancellationToken ct)
            => await _service.SaveSessionAsync(request, ct);

        [HttpPost]
        [InsertPermission]
        public async Task<Result> SaveSessions([FromBody] List<SavePomodoroSessionRequest> requests, CancellationToken ct)
            => await _service.SaveSessionsAsync(requests, ct);

        [HttpPost]
        [UpdatePermission]
        public async Task<Result> SaveSettings([FromBody] SavePomodoroSettingRequest request, CancellationToken ct)
            => await _service.SaveSettingAsync(request, ct);
    }
}
