using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class UserController : WebAdminControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [AccessPagePermission]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ViewPermission]
        public async Task<Result<List<UserModel>>> GetList(CancellationToken ct)
        {
            return await _userService.GetListUsersAsync(ct);
        }

        [HttpPost]
        [ViewPermission]
        public async Task<Result<UserModel>> GetUserById([FromBody] GetUserByIdRequest request, CancellationToken ct)
        {
            return await _userService.GetUserByIdAsync(request, ct);
        }
    }
}
