using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ManageLife.Controllers.Client
{
    public class AuthController : WebClientControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
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
        [EnableRateLimiting("login")]
        public async Task<Result> Register([FromBody] RegisterAccountRequest model, CancellationToken ct)
        {
            return await SignInWithCookieAsync(await _userService.RegisterAsync(model, ct));
        }

        [HttpPost]
        [EnableRateLimiting("login")]
        public async Task<Result> Login([FromBody] LoginAccountRequest model, CancellationToken ct)
        {
            return await SignInWithCookieAsync(await _userService.LoginAsync(model, ct));
        }

        [Authorize]
        [HttpPost]
        public async Task<Result> RefreshToken(CancellationToken ct)
        {
            var result = await _tokenService.RefreshTokenAsync(Request.Cookies["refreshToken"], ct);
            if (!result.IsOk())
                _tokenService.ClearTokensCookie();
            return await SignInWithCookieAsync(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<Result> Logout(CancellationToken ct)
        {
            var result = await _userService.LogoutAsync(Request.Cookies["refreshToken"], ct);
            _tokenService.ClearTokensCookie();
            return result;
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
            return await SignInWithCookieAsync(await _userService.ChangePasswordAsync(request, ct));
        }

        /// <summary>Set cookie phiên cho web khi thành công; không trả token ra JS.</summary>
        private async Task<Result> SignInWithCookieAsync(Result<AuthTokenModel> result)
        {
            if (!result.IsOk())
                return Result.Error(result.Code, result.Message, result.ErrorContent);

            await _tokenService.SetTokensCookieAsync(result.Data.AccessToken, result.Data.RefreshToken);
            return Result.Ok();
        }
    }
}
