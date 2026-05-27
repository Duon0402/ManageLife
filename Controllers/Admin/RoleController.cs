using ManageLife.Core;
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
        public async Task<Result<List<RoleModel>>> GetList(CancellationToken ct)
        {
            return await _service.GetListRolesAsync(ct);
        }

        [InsertPermission]
        [HttpPost]
        public async Task<Result> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
        {
            return await _service.CreateRoleAsync(request, ct);
        }

        [UpdatePermission]
        [HttpPost]
        public async Task<Result> Update([FromBody] UpdateRoleRequest request, CancellationToken ct)
        {
            return await _service.UpdateRoleAsync(request, ct);
        }

        [DeletePermission]
        [HttpPost]
        public async Task<Result> Delete([FromBody] DeleteRoleRequest request, CancellationToken ct)
        {
            return await _service.DeleteRoleAsync(request, ct);
        }
    }
}
