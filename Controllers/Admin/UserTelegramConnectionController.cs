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
        public async Task<Result<List<UserTelegramConnectionModel>>> GetListUserTelegramConnections()
        {
            var rs = await _service.GetListUserTelegramConnectionsAsync();
            return rs;
        }

        [ViewPermission]
        [HttpPost]
        public async Task<Result<UserTelegramConnectionModel>> GetUserTelegramConnectionByUserId(GetUserTelegramConnectionByUserIdRequest request)
        {
            var rs = await _service.GetUserTelegramConnectionByUserIdAsync(request);
            return rs;
        }

        [ViewPermission]
        [HttpPost]
        public async Task<Result<UserTelegramConnectionModel>> GetUserTelegramConnectionByChatId(GetUserTelegramConnectionByChatIdRequest request)
        {
            var rs = await _service.GetUserTelegramConnectionByChatIdAsync(request);
            return rs;
        }

        [InsertPermission]
        [HttpPost]
        public async Task<Result> CreateUserTelegramConnection(CreateUserTelegramConnectionRequest request)
        {
            var rs = await _service.CreateUserTelegramConnectionAsync(request);
            return rs;
        }

        [UpdatePermission]
        [HttpPost]
        public async Task<Result> UpdateUserTelegramConnection(UpdateUserTelegramConnectionRequest request)
        {
            var rs = await _service.UpdateUserTelegramConnectionAsync(request);
            return rs;
        }

        [DeletePermission]
        [HttpPost]
        public async Task<Result> DeleteUserTelegramConnection(DeleteUserTelegramConnectionRequest request)
        {
            var rs = await _service.DeleteUserTelegramConnectionAsync(request);
            return rs;
        }
    }
}