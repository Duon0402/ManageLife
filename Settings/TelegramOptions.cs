namespace ManageLife.Settings
{
    public class TelegramOptions
    {
        public const string Section = "TelegramSettings";
        public string BotToken { get; set; } = null!;
        public string? ChatId { get; set; }
        public string? ChatIdFileStorage { get; set; }
    }
}
