using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class TelegramBotCommandController : WebAdminControllerBase
    {
        private readonly ITelegramBotCommandService _commandService;
        private readonly ITelegramService _telegramService;

        public TelegramBotCommandController(ITelegramBotCommandService commandService, ITelegramService telegramService)
        {
            _commandService = commandService;
            _telegramService = telegramService;
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
    }
}
