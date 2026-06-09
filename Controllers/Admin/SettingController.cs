using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class SettingController : WebAdminControllerBase
    {
        private readonly ISettingService _service;

        public SettingController(ISettingService service)
        {
            _service = service;
        }

        [AccessPagePermission]
        public IActionResult Index() => View();

        [HttpGet]
        [ViewPermission]
        public async Task<Result<List<SettingModel>>> GetList(CancellationToken ct)
            => await _service.GetListSettingsAsync(new GetListSettingsRequest(), ct);

        [HttpPost]
        [UpdatePermission]
        public async Task<Result> Update([FromBody] UpdateSettingRequest request, CancellationToken ct)
            => await _service.UpdateSettingAsync(request, ct);
    }
}
