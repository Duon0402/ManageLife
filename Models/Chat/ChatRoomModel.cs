using ManageLife.Commons;

namespace ManageLife.Models
{
    public class ChatRoomModel
    {
        public string Id { get; set; } = default!;
        public RoomType Type { get; set; }
        public string? PrivateKey { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
