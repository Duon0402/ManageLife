using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Models;
using ManageLife.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers
{
    public class AuthController : WebControllerBase
    {
        private readonly UserService _service;

        public AuthController(AppDbContext context, IConfiguration config, IHttpContextAccessor httpContextAccessor, ILogger? logger = null) : base(context, logger)
        {
            _service = new UserService(context, config, httpContextAccessor);
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
            var rs = await _service.RegisterAsync(model);
            return rs;
        }

        [HttpPost]
        public async Task<Result> Login([FromBody] LoginAccountModel model)
        {
            var rs = await _service.LoginAsync(model);
            return rs;
        }

        [HttpPost]
        public async Task<Result> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var rs = await _service.RefreshTokenAsync(refreshToken);
            return rs;
        }
    }
}
