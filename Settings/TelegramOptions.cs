namespace ManageLife.Settings
{
    public class TelegramOptions
    {
        public const string Section = "TelegramSettings";
        public string BotToken { get; set; } = null!;
        public string? ChatId { get; set; }
        public string? ChatIdFileStorage { get; set; }
        /// <summary>
        /// Secret token đăng ký kèm webhook; Telegram gửi lại qua header X-Telegram-Bot-Api-Secret-Token.
        /// Chưa cấu hình thì webhook từ chối mọi request (1–256 ký tự A-Z a-z 0-9 _ -).
        /// </summary>
        public string? WebhookSecret { get; set; }
    }
}
