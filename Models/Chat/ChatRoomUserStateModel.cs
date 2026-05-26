namespace ManageLife.Models
{
    public class ChatRoomUserStateModel
    {
        public string RoomId { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string? LastReadMessageId { get; set; }
        public DateTime? LastReadAt { get; set; }
    }
}
