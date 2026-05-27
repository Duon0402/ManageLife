namespace ManageLife.Models
{
    public class UserTelegramConnectionModel
    {
        public string Id { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public long ChatId { get; set; }
    }
}
