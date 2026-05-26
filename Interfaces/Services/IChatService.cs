using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IChatService
    {
        Task<Result<string>> CreateOrGetPrivateRoomAsync(string user1, string user2, CancellationToken ct = default);

        Task<Result<ChatMessageModel>> SendMessageAsync(string roomId, string senderId, string content, CancellationToken ct = default);

        Task<Result<List<ChatMessageModel>>> GetMessagesAsync(string roomId, DateTime? before, int pageSize, CancellationToken ct = default);

        Task<Result> MarkAsReadAsync(string userId, string roomId, CancellationToken ct = default);

        Task<Result<int>> GetUnreadCountAsync(string roomId, string userId, CancellationToken ct = default);

        Task<bool> IsMemberAsync(string roomId, string userId, CancellationToken ct = default);
    }
}
