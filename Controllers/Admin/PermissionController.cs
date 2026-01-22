using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class PermissionController : WebAdminControllerBase
    {
        private readonly IPermissionService _service;

        public PermissionController(AppDbContext context, IPermissionService service, ILogger? logger = null) : base(context, logger)
        {
            _service = service;
        }

        [AccessPagePermission]
        public IActionResult Index()
        {
            return View();
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
