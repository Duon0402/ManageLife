using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.API
{
    [Authorize]
    [Route("api/account")]
    public class AccountApiController : ApiControllerBase
    {
        private readonly IUserService _userService;

        public AccountApiController(IUserService userService)
        {
            _userService = userService;
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
    }
}
