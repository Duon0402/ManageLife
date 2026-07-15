using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class SettingController : WebAdminControllerBase
    {
        private readonly ISettingService _service;
        private readonly IEmailService _emailService;

        public SettingController(ISettingService service, IEmailService emailService)
        {
            _service = service;
            _emailService = emailService;
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

        [HttpPost]
        [UpdatePermission]
        public async Task<Result> SendTestEmail([FromBody] SendTestEmailRequest request, CancellationToken ct)
            => await _emailService.SendAsync(request.To, "Test email từ Manage Life", "<p>Đây là email thử nghiệm cấu hình SMTP.</p>", true, ct);
    }
}
