using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class UserTelegramConnectionController : WebAdminControllerBase
    {
        private readonly IUserTelegramConnectionService _service;

        public UserTelegramConnectionController(IUserTelegramConnectionService service)
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
        public async Task<Result<List<UserTelegramConnectionModel>>> GetList(CancellationToken ct)
        {
            return await _service.GetListUserTelegramConnectionsAsync(ct);
        }

        [ViewPermission]
        [HttpPost]
        public async Task<Result<UserTelegramConnectionModel>> GetByUserId(GetUserTelegramConnectionByUserIdRequest request, CancellationToken ct)
        {
            return await _service.GetUserTelegramConnectionByUserIdAsync(request, ct);
        }

        [ViewPermission]
        [HttpPost]
        public async Task<Result<UserTelegramConnectionModel>> GetByChatId(GetUserTelegramConnectionByChatIdRequest request, CancellationToken ct)
        {
            return await _service.GetUserTelegramConnectionByChatIdAsync(request, ct);
        }

        [InsertPermission]
        [HttpPost]
        public async Task<Result> Create(CreateUserTelegramConnectionRequest request, CancellationToken ct)
        {
            return await _service.CreateUserTelegramConnectionAsync(request, ct);
        }

        [UpdatePermission]
        [HttpPost]
        public async Task<Result> Update(UpdateUserTelegramConnectionRequest request, CancellationToken ct)
        {
            return await _service.UpdateUserTelegramConnectionAsync(request, ct);
        }

        [DeletePermission]
        [HttpPost]
        public async Task<Result> Delete(DeleteUserTelegramConnectionRequest request, CancellationToken ct)
        {
            return await _service.DeleteUserTelegramConnectionAsync(request, ct);
        }
    }
}
