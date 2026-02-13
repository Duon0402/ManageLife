using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class UserController : WebAdminControllerBase
    {
        private readonly IUserService _userService;

        public UserController(AppDbContext context, IUserService userService) : base(context)
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
        public async Task<Result<List<UserModel>>> GetListUsers()
        {
            var rs = await _userService.GetListUsersAsync();
            return rs;
        }

        [HttpPost]
        [ViewPermission]
        public async Task<Result<UserModel>> GetUserById([FromBody] GetUserByIdRequest request)
        {
            var rs = await _userService.GetUserByIdAsync(request);
            return rs;
        }
    }
}
