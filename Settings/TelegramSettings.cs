namespace ManageLife.Settings
{
    public class TelegramSettings
    {
        public const string Section = "TelegramSettings";
        public string BotToken { get; set; } = null!;
        public string? ChatId { get; set; }
        public string? ChatIdFileStorage { get; set; }
    }
}
