using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IUserTelegramConnectionService
    {
        Task<Result<List<UserTelegramConnectionModel>>> GetListUserTelegramConnectionsAsync(CancellationToken ct = default);
        Task<Result<UserTelegramConnectionModel>> GetUserTelegramConnectionByUserIdAsync(GetUserTelegramConnectionByUserIdRequest request, CancellationToken ct = default);
        Task<Result<UserTelegramConnectionModel>> GetUserTelegramConnectionByChatIdAsync(GetUserTelegramConnectionByChatIdRequest request, CancellationToken ct = default);
        Task<Result> CreateUserTelegramConnectionAsync(CreateUserTelegramConnectionRequest request, CancellationToken ct = default);
        Task<Result> UpdateUserTelegramConnectionAsync(UpdateUserTelegramConnectionRequest request, CancellationToken ct = default);
        Task<Result> DeleteUserTelegramConnectionAsync(DeleteUserTelegramConnectionRequest request, CancellationToken ct = default);
    }
}
