using ManageLife.Base;

namespace ManageLife.Entities
{
    public class ChatRoomUserStateEntity : EntityBase, ICanCreate, ICanUpdate
    {
        public string RoomId { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string? LastReadMessageId { get; set; }
        public DateTime? LastReadAt { get; set; }

        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
