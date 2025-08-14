using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers
{
    public class TelegramAPIController : ApiControllerBase
    {
        private readonly TelegramService _service;

        public TelegramAPIController(AppDbContext context, IConfiguration config, ILogger? logger = null) : base(context, logger)
        {
            _service = new TelegramService(config);
        }

        [HttpGet("notify")]
        public async Task<Result<string>> SendNotification()
        {
            try
            {
                await _service.SendMessageAsync("Thông báo hàng ngày");
                return Result.Ok("Đã gửi tin nhắn Telegram");
            }
            catch (Exception ex)
            {
                return Result.Exception<string>("Gửi tin nhắn thất bại", ex);
            }
        }
    }
}
