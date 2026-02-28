using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IChatService
    {
        Task<Result<string>> CreateOrGetPrivateRoomAsync(string user1, string user2);

        Task<Result<ChatMessageModel>> SendMessageAsync(string roomId, string senderId, string content);

        Task<Result<List<ChatMessageModel>>> GetMessagesAsync(string roomId, DateTime? before, int pageSize);

        Task<Result> MarkAsReadAsync(string userId, string roomId);

        Task<Result<int>> GetUnreadCountAsync(string roomId, string userId);

        Task<bool> IsMemberAsync(string roomId, string userId);
    }
}
