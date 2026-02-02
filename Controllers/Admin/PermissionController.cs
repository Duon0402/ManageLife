using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class PermissionController : WebAdminControllerBase
    {
        private readonly IPermissionService _service;
        private readonly IUserService _userService;

        public PermissionController(AppDbContext context, IPermissionService service, IUserService userService, ILogger? logger = null) : base(context, logger)
        {
            _service = service;
            _userService = userService;
        }

        [AccessPagePermission]
        public async Task<IActionResult> Index(string userId)
        {
            var viewModel = new AdminPermissionViewModel();
            var rsUser = await _userService.GetUserIdAsync(new GetUserIdRequest { UserId = userId });

            if (rsUser.IsOk() && rsUser.Data != null)
            {
                viewModel.UserId = rsUser.Data.Id;
                viewModel.UserName = rsUser.Data.UserName;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ViewPermission]
        public async Task<Result<List<PermissionModel>>> GetAssignedPermissionsByUserId([FromBody] GetAssignedPermissionsByUserIdRequest request)
        {
            var rs = await _service.GetAssignedPermissionsByUserIdAsync(request);
            return rs;
        }

        [HttpPost]
        [ViewPermission]
        public async Task<Result<List<PermissionModel>>> GetUnassignedPermissionsByUserId([FromBody] GetUnassignedPermissionsByUserIdRequest request)
        {
            var rs = await _service.GetUnassignedPermissionsByUserIdAsync(request);
            return rs;
        }

        [HttpPost]
        [Permission("AssignPermissions")]
        public async Task<Result> AssignPermissions([FromBody] AssignPermissionsRequest request)
        {
            var rs = await _service.AssignPermissionsAsync(request);
            return rs;
        }

        [HttpPost]
        [Permission("UnassignPermissions")]
        public async Task<Result> UnassignPermissions([FromBody] UnassignPermissionsRequest request)
        {
            var rs = await _service.UnassignPermissionsAsync(request);
            return rs;
        }
    }
}
