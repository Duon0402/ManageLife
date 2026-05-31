namespace ManageLife.Models
{
    public class TelegramBotCommandModel
    {
        public string Id { get; set; } = default!;
        public string Command { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int SortOrder { get; set; }
    }
}
