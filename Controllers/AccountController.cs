using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Models;
using ManageLife.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers
{
    public class AccountController : WebControllerBase
    {
        private readonly UserService _service;

        public AccountController(AppDbContext context, ILogger? logger = null) : base(context, logger)
        {
            _service = new UserService(context);
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
        public async Task<IActionResult> Register(RegisterAccountModel model)
        {
            var rs = await _service.RegisterAsync(model);

            if (rs.IsOk())
            {
                return Redirect("Login");
            }

            return Ok(rs);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginAccountModel model)
        {
            var rs = await _service.LoginAsync(model);

            if (rs.IsOk())
            {
                return RedirectToAction("Index", "Home");
            }

            return Ok(rs);
        }
    }
}
