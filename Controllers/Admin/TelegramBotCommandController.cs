using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ManageLife.Controllers.Admin
{
    public class TelegramBotCommandController : WebAdminControllerBase
    {
        private readonly ITelegramBotCommandService _commandService;
        private readonly ITelegramService _telegramService;
        private readonly CronJobOptions _cronOptions;

        public TelegramBotCommandController(
            ITelegramBotCommandService commandService,
            ITelegramService telegramService,
            IOptions<CronJobOptions> cronOptions)
        {
            _commandService = commandService;
            _telegramService = telegramService;
            _cronOptions = cronOptions.Value;
        }

        [AccessPagePermission]
        public IActionResult Index()
        {
            return View();
        }

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<TelegramBotCommandModel>>> GetList(CancellationToken ct)
        {
            return await _commandService.GetListAsync(ct);
        }

        [InsertPermission]
        [HttpPost]
        public async Task<Result> Create([FromBody] CreateTelegramBotCommandRequest request, CancellationToken ct)
        {
            return await _commandService.CreateAsync(request, ct);
        }

        [UpdatePermission]
        [HttpPost]
        public async Task<Result> Update([FromBody] UpdateTelegramBotCommandRequest request, CancellationToken ct)
        {
            return await _commandService.UpdateAsync(request, ct);
        }

        [DeletePermission]
        [HttpPost]
        public async Task<Result> Delete([FromBody] DeleteTelegramBotCommandRequest request, CancellationToken ct)
        {
            return await _commandService.DeleteAsync(request, ct);
        }

        [UpdatePermission]
        [HttpPost]
        public async Task<Result> SyncToTelegram(CancellationToken ct)
        {
            return await _telegramService.SetDefaultCommandsAsync(ct);
        }

        /// <summary>
        /// Đăng ký webhook của bot về {CronJob:AppBaseUrl}/api/telegram/webhook kèm TelegramSettings:WebhookSecret — bấm lại
        /// sau khi đổi secret hoặc đổi domain (không thì Telegram gửi secret cũ → webhook trả 401).
        /// </summary>
        [UpdatePermission]
        [HttpPost]
        public async Task<Result<string>> SetWebhook(CancellationToken ct)
        {
            var baseUrl = (_cronOptions.AppBaseUrl ?? "").Trim().TrimEnd('/');
            if (baseUrl.IsEmpty())
                return Result.Error<string>(Result.DATA_INVALID.Code, "Chưa cấu hình CronJob:AppBaseUrl");
            return await _telegramService.RegisterWebhookAsync($"{baseUrl}/api/telegram/webhook", ct);
        }

        /// <summary>Thông tin webhook Telegram đang giữ (URL, số tin chờ, lỗi gần nhất).</summary>
        [ViewPermission]
        [HttpGet]
        public async Task<Result<object>> WebhookStatus(CancellationToken ct)
        {
            return await _telegramService.GetWebhookStatusAsync(ct);
        }
    }
}
