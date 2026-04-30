using ManageLife.Core;

namespace ManageLife.Entities
{
    public class ChatMessageEntity : EntityBase, ICanCreate
    {
        public string RoomId { get; set; } = default!;

        public string SenderId { get; set; } = default!;

        public string Content { get; set; } = default!;

        public string CreatedUser { get; set; } = default!;

        public DateTime CreatedTime { get; set; }
    }
}
