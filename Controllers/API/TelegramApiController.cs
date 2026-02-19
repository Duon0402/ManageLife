using ManageLife.Base;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.API
{
    [Route("api/telegram")]
    public class TelegramApiController : ApiControllerBase
    {
        private readonly ITelegramService _service;

        public TelegramApiController(ITelegramService service)
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
