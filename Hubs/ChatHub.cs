using ManageLife.Contexts;
using ManageLife.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ManageLife.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _service;

        public ChatHub(IChatService service)
        {
            _service = service;
        }

        private static string GetUserId()
        {
            var userId = UserContext.GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
                throw new HubException("User not authenticated");

            return userId;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomId)
        {
            var userId = GetUserId();

            var isMember = await _service.IsMemberAsync(roomId, userId);

            if (!isMember)
                throw new HubException("Không có quyền truy cập room này.");

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }

        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        }
        public async Task SendMessage(string roomId, string content)
        {
            var userId = GetUserId();

            if (string.IsNullOrWhiteSpace(content))
                throw new HubException("Tin nhắn không hợp lệ.");

            var result = await _service.SendMessageAsync(roomId, userId, content);
            if (!result.IsOk())
                throw new HubException(result.Message);

            await Clients.Group(roomId).SendAsync("ReceiveMessage", result.Data);
        }

        public async Task MarkAsRead(string roomId)
        {
            var userId = GetUserId();

            var isMember = await _service.IsMemberAsync(roomId, userId);
            if (!isMember)
                throw new HubException("Không có quyền.");

            await _service.MarkAsReadAsync(userId, roomId);

            await Clients.Group(roomId).SendAsync("MessagesRead", new
            {
                RoomId = roomId,
                UserId = userId
            });
        }

        public async Task Typing(string roomId, bool isTyping)
        {
            var userId = GetUserId();

            var isMember = await _service.IsMemberAsync(roomId, userId);
            if (!isMember) return;

            await Clients.OthersInGroup(roomId).SendAsync("UserTyping", new
            {
                RoomId = roomId,
                UserId = userId,
                IsTyping = isTyping
            });
        }
    }
}