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

        public AuthController(AppDbContext context, IConfiguration config, ILogger? logger = null) : base(context, logger)
        {
            _service = new UserService(context, config);
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterAccountModel model)
        {
            var rs = await _service.RegisterAsync(model);
            return Json(rs);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginAccountModel model)
        {
            var rs = await _service.LoginAsync(model);
            return Json(rs);
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            var rs = await _service.RefreshTokenAsync(model.RefreshToken);
            return Json(rs);
        }
    }
}
