using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.API
{
    [Route("api/telegram")]
    public class TelegramApiController : ApiControllerBase
    {
        private readonly TelegramService _service;

        public TelegramApiController(AppDbContext context, IConfiguration config, ILogger? logger = null) : base(context, logger)
        {
            _service = new TelegramService(config);
        }

        [HttpGet("send-notification")]
        public async Task<IActionResult> SendNotification()
        {

            var rs = await _service.SendMessageAsync("Thông báo hàng ngày");
            if (rs.IsOk())
            {
                return Ok();
            }

            return BadRequest(rs.Message);
        }
    }
}
