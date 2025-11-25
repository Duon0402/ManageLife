using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.API
{
    [Route("api/telegram")]
    public class TelegramApiController : ApiControllerBase
    {
        private readonly ITelegramService _service;

        public TelegramApiController(AppDbContext context, ITelegramService service, ILogger? logger = null) : base(context, logger)
        {
            _service = service;
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var rs = await _service.SendMessageAsync(request);
            if (rs.IsOk())
            {
                return Ok();
            }

            return BadRequest(rs.Message);
        }
    }
}
