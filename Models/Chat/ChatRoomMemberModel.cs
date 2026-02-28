namespace ManageLife.Models
{
    public class ChatRoomMemberModel
    {
        public string RoomId { get; set; } = default!;

        public string UserId { get; set; } = default!;

        public DateTime JoinedAt { get; set; }

        public bool IsActive { get; set; }
    }
}
