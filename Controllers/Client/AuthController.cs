using ManageLife.Base;
using ManageLife.Data;
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

        public AuthController(AppDbContext context, IUserService userService, ITokenService tokenService, ILogger? logger = null) : base(context, logger)
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
        public async Task<Result> Register([FromBody] RegisterAccountRequest model)
        {
            var rs = await _userService.RegisterAsync(model);
            return rs;
        }

        [HttpPost]
        public async Task<Result> Login([FromBody] LoginAccountRequest model)
        {
            var rs = await _userService.LoginAsync(model);
            return rs;
        }

        [Authorize]
        [HttpPost]
        public async Task<Result> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var rs = await _tokenService.RefreshTokenAsync(refreshToken);
            return rs;
        }

        [Authorize]
        [HttpPost]
        public async Task<Result> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var rs = await _userService.LogoutAsync(refreshToken);
            return rs;
        }

        [Authorize]
        [HttpPost]
        public async Task<Result> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var rs = await _userService.ChangePasswordAsync(request, refreshToken);
            return rs;
        }
    }
}
