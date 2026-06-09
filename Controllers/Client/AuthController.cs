using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class AuthController : WebClientControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly ISettingContext _settingContext;

        public AuthController(IUserService userService, ITokenService tokenService, ISettingContext settingContext)
        {
            _userService = userService;
            _tokenService = tokenService;
            _settingContext = settingContext;
        }

        public IActionResult Login()
        {
            return View("Login");
        }

        public IActionResult Register()
        {
            return View("Register");
        }

        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }

        [HttpPost]
        public async Task<Result> Register([FromBody] RegisterAccountRequest model, CancellationToken ct)
        {
            if (!await _settingContext.GetBoolAsync(SettingKeys.Feature.EnableRegistration, true))
                return Result.Error("FEATURE_DISABLED", "Đăng ký tài khoản hiện đang tạm ngưng");
            return await _userService.RegisterAsync(model, ct);
        }

        [HttpPost]
        public async Task<Result> Login([FromBody] LoginAccountRequest model, CancellationToken ct)
        {
            return await _userService.LoginAsync(model, ct);
        }

        [Authorize]
        [HttpPost]
        public async Task<Result> RefreshToken(CancellationToken ct)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            return await _tokenService.RefreshTokenAsync(refreshToken, ct);
        }

        [Authorize]
        [HttpPost]
        public async Task<Result> Logout(CancellationToken ct)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            return await _userService.LogoutAsync(refreshToken, ct);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View("ChangePassword");
        }

        [Authorize]
        [HttpPost]
        public async Task<Result> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            return await _userService.ChangePasswordAsync(request, refreshToken, ct);
        }
    }
}
