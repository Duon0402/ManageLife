using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Telegram.Bot.Types;

namespace ManageLife.Controllers.API
{
    [Route("api/telegram")]
    [AllowAnonymous]
    public class TelegramApiController : ApiControllerBase
    {
        private readonly ITelegramService _service;
        private readonly IAppLogger<TelegramApiController> _logger;

        public TelegramApiController(ITelegramService service, IAppLogger<TelegramApiController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] SendTelegramMessageRequest request, CancellationToken ct)
        {
            var rs = await _service.SendMessageAsync(request, ct);
            if (rs.IsOk())
                return Ok();

            return BadRequest(rs.Message);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] JsonElement json, CancellationToken ct)
        {
            try
            {
                var jsonString = json.GetRawText();
                var update = JsonSerializer.Deserialize<Update>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (update != null)
                {
                    await _service.HandleUpdateAsync(update, ct);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                var msg = "Đã có lỗi xảy ra khi xử lý webhook từ Telegram";
                _logger.Error(ex, msg);
                return Ok();
            }
        }

        [HttpGet("set-webhook")]
        public async Task<IActionResult> SetWebhook(string url, CancellationToken ct)
        {
            var rs = await _service.RegisterWebhookAsync(url, ct);
            if (rs.IsOk())
                return Ok(rs.Data);

            return BadRequest(rs.Message);
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus(CancellationToken ct)
        {
            var rs = await _service.GetWebhookStatusAsync(ct);
            return Ok(rs);
        }

        [HttpGet("register-commands")]
        public async Task<IActionResult> RegisterCommands(CancellationToken ct)
        {
            var rs = await _service.SetDefaultCommandsAsync(ct);
            if (rs.IsOk())
                return Ok(rs.Message);

            return BadRequest(rs.Message);
        }
    }
}
