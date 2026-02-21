using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IUserTelegramConnectionService
    {
        Task<Result<List<UserTelegramConnectionModel>>> GetListUserTelegramConnectionsAsync();
        Task<Result<UserTelegramConnectionModel>> GetUserTelegramConnectionByUserIdAsync(GetUserTelegramConnectionByUserIdRequest request);
        Task<Result<UserTelegramConnectionModel>> GetUserTelegramConnectionByChatIdAsync(GetUserTelegramConnectionByChatIdRequest request);
        Task<Result> CreateUserTelegramConnectionAsync(CreateUserTelegramConnectionRequest request);
        Task<Result> UpdateUserTelegramConnectionAsync(UpdateUserTelegramConnectionRequest request);
        Task<Result> DeleteUserTelegramConnectionAsync(DeleteUserTelegramConnectionRequest request);
    }
}
