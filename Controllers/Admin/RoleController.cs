using ManageLife.Base;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class RoleController : WebAdminControllerBase
    {
        private readonly IRoleService _service;

        public RoleController(IRoleService service)
        {
            _service = service;
        }

        [AccessPagePermission]
        public IActionResult Index()
        {
            return View();
        }

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<RoleModel>>> GetListRoles()
        {
            var rs = await _service.GetListRolesAsync();
            return rs;
        }

        [InsertPermission]
        [HttpPost]
        public async Task<Result> CreateRole([FromBody] CreateRoleRequest request)
        {
            var rs = await _service.CreateRoleAsync(request);
            return rs;
        }

        [DeletePermission]
        [HttpPost]
        public async Task<Result> DeleteRole([FromBody] DeleteRoleRequest request)
        {
            var rs = await _service.DeleteRoleAsync(request);
            return rs;
        }
    }
}
