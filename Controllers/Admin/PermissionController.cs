using ManageLife.Base;
using ManageLife.Commons;
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
        private readonly IRoleService _roleService;

        public PermissionController(AppDbContext context, IPermissionService service, IUserService userService, IRoleService roleService) : base(context)
        {
            _service = service;
            _userService = userService;
            _roleService = roleService;
        }

        [AccessPagePermission]
        public async Task<IActionResult> IndexByUser(string userId)
        {
            var viewModel = new AdminPermissionViewModel
            {
                TargetType = PermissionTargetType.User
            };

            var rsUser = await _userService.GetUserByIdAsync(new GetUserByIdRequest { UserId = userId });

            if (rsUser.IsOk() && rsUser.Data != null)
            {
                viewModel.UserId = rsUser.Data.Id;
                viewModel.UserName = rsUser.Data.UserName;
            }

            return View("Index", viewModel);
        }

        [AccessPagePermission]
        public async Task<IActionResult> IndexByRole(string roleId)
        {
            var viewModel = new AdminPermissionViewModel
            {
                TargetType = PermissionTargetType.Role
            };

            var rsRole = await _roleService.GetRoleByIdAsync(new GetRoleByIdRequest { RoleId = roleId });

            if (rsRole.IsOk() && rsRole.Data != null)
            {
                viewModel.RoleId = rsRole.Data.Id;
                viewModel.RoleName = rsRole.Data.Name;
            }
            return View("Index", viewModel);
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

        [HttpPost]
        [ViewPermission]
        public async Task<Result<List<PermissionModel>>> GetAssignedPermissionsByRoleId([FromBody] GetAssignedPermissionsByRoleIdRequest request)
        {
            var rs = await _service.GetAssignedPermissionsByRoleIdAsync(request);
            return rs;
        }

        [HttpPost]
        [ViewPermission]
        public async Task<Result<List<PermissionModel>>> GetUnAssignedPermissionsByRoleId([FromBody] GetUnAssignedPermissionsByRoleIdRequest request)
        {
            var rs = await _service.GetUnAssignedPermissionsByRoleIdAsync(request);
            return rs;
        }
    }
}
