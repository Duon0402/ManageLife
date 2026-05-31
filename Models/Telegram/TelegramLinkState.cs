namespace ManageLife.Models
{
    public class TelegramLinkState
    {
        public string Step { get; set; } = TelegramLinkStep.WaitingUsername;
        public string? Username { get; set; }
    }

    public static class TelegramLinkStep
    {
        public const string WaitingUsername = "waiting_username";
        public const string WaitingPassword = "waiting_password";
    }
}
