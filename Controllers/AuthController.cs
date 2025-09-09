using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers
{
    public class AuthController : WebControllerBase
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
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        public async Task<Result> Register([FromBody] RegisterAccountModel model)
        {
            var rs = await _userService.RegisterAsync(model);
            return rs;
        }

        [HttpPost]
        public async Task<Result> Login([FromBody] LoginAccountModel model)
        {
            var rs = await _userService.LoginAsync(model);
            return rs;
        }

        [HttpPost]
        public async Task<Result> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var rs = await _tokenService.RefreshTokenAsync(refreshToken);
            return rs;
        }
    }
}
