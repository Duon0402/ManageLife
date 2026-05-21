namespace ManageLife.Settings
{
    public class RedisSettings
    {
        public const string Section = "Redis";
        public string EndPoints { get; set; } = null!;
        public string User { get; set; } = "default";
        public string Password { get; set; } = "";
    }
}
