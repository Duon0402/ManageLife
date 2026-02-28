using ManageLife.Base;
using ManageLife.Commons;

namespace ManageLife.Entities
{
    public class ChatRoomEntity : EntityBase, ICanCreate
    {
        public RoomType Type { get; set; }
        public string? PrivateKey { get; set; }
        
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
    }
}
