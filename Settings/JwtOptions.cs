namespace ManageLife.Settings
{
    public class JwtOptions
    {
        public const string Section = "Jwt";
        public string Key { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
    }
}
