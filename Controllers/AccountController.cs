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
	}
}
