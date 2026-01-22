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
        public async Task<Result<List<PermissionModel>>> GetAssignedPermissionsByUserId(GetAssignedPermissionsByUserIdRequest request)
        {
            var rs = await _service.GetAssignedPermissionsByUserIdAsync(request);
            return rs;
        }

        [HttpPost]
        [ViewPermission]
        public async Task<Result<List<PermissionModel>>> GetUnassignedPermissionsByUserId(GetUnassignedPermissionsByUserIdRequest request)
        {
            var rs = await _service.GetUnassignedPermissionsByUserIdAsync(request);
            return rs;
        }
    }
}
