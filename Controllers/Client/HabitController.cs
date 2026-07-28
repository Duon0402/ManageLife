using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class HabitController : WebClientControllerBase
    {
        private readonly IHabitService _service;
        private readonly ISettingContext _settingContext;

        public HabitController(IHabitService service, ISettingContext settingContext)
        {
            _service = service;
            _settingContext = settingContext;
        }

        [AccessPagePermission]
        public async Task<IActionResult> Index()
        {
            if (!await _settingContext.GetBoolAsync(SettingKeys.Feature.EnableHabit, true))
                return NotFound();
            return View();
        }

        [HttpGet]
        [ViewPermission]
        public async Task<Result<List<HabitModel>>> GetList(CancellationToken ct)
            => await _service.GetListAsync(ct);

        [HttpPost]
        [InsertPermission]
        public async Task<Result> Create([FromBody] CreateHabitRequest request, CancellationToken ct)
            => await _service.CreateAsync(request, ct);

        [HttpPost]
        [UpdatePermission]
        public async Task<Result> Update([FromBody] UpdateHabitRequest request, CancellationToken ct)
            => await _service.UpdateAsync(request, ct);

        [HttpPost]
        [DeletePermission]
        public async Task<Result> Delete([FromBody] DeleteHabitRequest request, CancellationToken ct)
            => await _service.DeleteAsync(request, ct);
    }
}
