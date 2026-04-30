using ManageLife.Core;

namespace ManageLife.Entities
{
    public class ChatRoomMemberEntity : EntityBase, ICanCreate, ICanUpdate
    {
        public string RoomId { get; set; } = default!;

        public string UserId { get; set; } = default!;

        public bool IsActive { get; set; }

        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
