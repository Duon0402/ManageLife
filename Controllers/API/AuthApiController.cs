using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ManageLife.Controllers.API
{
    [Route("api/auth")]
    public class AuthApiController : ApiControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IDeviceCredentialService _deviceCredentialService;

        public AuthApiController(
            IUserService userService,
            ITokenService tokenService,
            IDeviceCredentialService deviceCredentialService)
        {
            _userService = userService;
            _tokenService = tokenService;
            _deviceCredentialService = deviceCredentialService;
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<Result<AuthTokenModel>> Login([FromBody] LoginAccountRequest model, CancellationToken ct)
        {
            return await _userService.LoginAsync(model, ct);
        }

        [HttpPost("register")]
        [EnableRateLimiting("login")]
        public async Task<Result<AuthTokenModel>> Register([FromBody] RegisterAccountRequest model, CancellationToken ct)
        {
            return await _userService.RegisterAsync(model, ct);
        }

        /// <summary>Đăng nhập bằng khoá thiết bị (vân tay/khuôn mặt đã bật ở máy này): dùng được cả khi đã đăng xuất.</summary>
        [HttpPost("device-login")]
        [EnableRateLimiting("login")]
        public async Task<Result<AuthTokenModel>> DeviceLogin([FromBody] DeviceLoginRequest model, CancellationToken ct)
        {
            return await _deviceCredentialService.LoginAsync(model, ct);
        }

        [HttpPost("logout")]
        public async Task<Result> Logout([FromBody] RefreshTokenRequest model, CancellationToken ct)
        {
            return await _userService.LogoutAsync(model.RefreshToken, ct);
        }

        [HttpPost("refresh")]
        public async Task<Result<AuthTokenModel>> Refresh([FromBody] RefreshTokenRequest model, CancellationToken ct)
        {
            return await _tokenService.RefreshTokenAsync(model.RefreshToken, ct);
        }
    }
}
