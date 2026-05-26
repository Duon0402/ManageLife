namespace ManageLife.Models
{
    public class ChatMessageModel
    {
        public string Id { get; set; } = default!;

        public string RoomId { get; set; } = default!;

        public string SenderId { get; set; } = default!;

        public string Content { get; set; } = default!;

        public DateTime CreatedTime { get; set; }
    }
}
