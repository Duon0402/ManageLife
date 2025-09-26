namespace ManageLife.Models
{
    public class ChangeLanguageRequest
    {
        public string LanguageCode { get; set; } = null!;
        public string? ReturnUrl { get; set; }
    }
}
