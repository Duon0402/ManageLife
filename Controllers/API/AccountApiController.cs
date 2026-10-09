using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ManageLife.Controllers.API
{
    [Authorize]
    [Route("api/account")]
    public class AccountApiController : ApiControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITelegramService _telegramService;

        public AccountApiController(IUserService userService, ITelegramService telegramService)
        {
            _userService = userService;
            _telegramService = telegramService;
        }

        [HttpGet("me")]
        public async Task<Result<AccountModel>> GetMe(CancellationToken ct)
        {
            return await _userService.GetMyAccountAsync(ct);
        }

        /// <summary>Thay toàn bộ thông tin cá nhân: client luôn gửi đủ FullName và Email (field trống = xoá).</summary>
        [HttpPut("me")]
        public async Task<Result<AccountModel>> UpdateMe([FromBody] UpdateAccountRequest request, CancellationToken ct)
        {
            return await _userService.UpdateMyAccountAsync(request, ct);
        }

        /// <summary>Đổi mật khẩu: mọi phiên khác bị đăng xuất, trả về cặp token mới cho thiết bị hiện tại.</summary>
        [HttpPost("change-password")]
        public async Task<Result<AuthTokenModel>> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
        {
            return await _userService.ChangePasswordAsync(request, ct);
        }

        /// <summary>Tạo deep link mở bot Telegram kèm mã dùng 1 lần (5 phút); bot nhận /start &lt;mã&gt; thì liên kết.</summary>
        [HttpPost("telegram/link")]
        [EnableRateLimiting("telegram-link")]
        public async Task<Result<TelegramLinkModel>> CreateTelegramLink(CancellationToken ct)
        {
            return await _telegramService.CreateLinkAsync(ct);
        }

        [HttpDelete("telegram")]
        public async Task<Result> UnlinkTelegram(CancellationToken ct)
        {
            return await _telegramService.UnlinkAsync(ct);
        }
    }
}
