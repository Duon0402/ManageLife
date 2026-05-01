using ManageLife.Core;
using ManageLife.Commons;
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

        public PermissionController(IPermissionService service, IUserService userService, IRoleService roleService)
        {
            _service = service;
            _userService = userService;
            _roleService = roleService;
        }

        [AccessPagePermission]
        public async Task<IActionResult> IndexByUser(string userId, CancellationToken ct)
        {
            var viewModel = new AdminPermissionViewModel
            {
                TargetType = PermissionTargetType.User
            };

            var rsUser = await _userService.GetUserByIdAsync(new GetUserByIdRequest { UserId = userId }, ct);

            if (rsUser.IsOk() && rsUser.Data != null)
            {
                viewModel.UserId = rsUser.Data.Id;
                viewModel.UserName = rsUser.Data.UserName;
            }

            return View("Index", viewModel);
        }

        [AccessPagePermission]
        public async Task<IActionResult> IndexByRole(string roleId, CancellationToken ct)
        {
            var viewModel = new AdminPermissionViewModel
            {
                TargetType = PermissionTargetType.Role
            };

            var rsRole = await _roleService.GetRoleByIdAsync(new GetRoleByIdRequest { RoleId = roleId }, ct);

            if (rsRole.IsOk() && rsRole.Data != null)
            {
                viewModel.RoleId = rsRole.Data.Id;
                viewModel.RoleName = rsRole.Data.Name;
            }
            return View("Index", viewModel);
        }

        [HttpPost]
        [ViewPermission]
        public async Task<Result<List<PermissionModel>>> GetAssignedPermissionsByUserId([FromBody] GetAssignedPermissionsByUserIdRequest request, CancellationToken ct)
        {
            return await _service.GetAssignedPermissionsByUserIdAsync(request, ct);
        }

        [HttpPost]
        [ViewPermission]
        public async Task<Result<List<PermissionModel>>> GetUnassignedPermissionsByUserId([FromBody] GetUnassignedPermissionsByUserIdRequest request, CancellationToken ct)
        {
            return await _service.GetUnassignedPermissionsByUserIdAsync(request, ct);
        }

        [HttpPost]
        [Permission("AssignPermissions")]
        public async Task<Result> AssignPermissions([FromBody] AssignPermissionsRequest request, CancellationToken ct)
        {
            return await _service.AssignPermissionsAsync(request, ct);
        }

        [HttpPost]
        [Permission("UnassignPermissions")]
        public async Task<Result> UnassignPermissions([FromBody] UnassignPermissionsRequest request, CancellationToken ct)
        {
            return await _service.UnassignPermissionsAsync(request, ct);
        }

        [HttpPost]
        [ViewPermission]
        public async Task<Result<List<PermissionModel>>> GetAssignedPermissionsByRoleId([FromBody] GetAssignedPermissionsByRoleIdRequest request, CancellationToken ct)
        {
            return await _service.GetAssignedPermissionsByRoleIdAsync(request, ct);
        }

        [HttpPost]
        [ViewPermission]
        public async Task<Result<List<PermissionModel>>> GetUnAssignedPermissionsByRoleId([FromBody] GetUnAssignedPermissionsByRoleIdRequest request, CancellationToken ct)
        {
            return await _service.GetUnAssignedPermissionsByRoleIdAsync(request, ct);
        }
    }
}
