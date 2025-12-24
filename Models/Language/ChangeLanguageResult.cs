using ManageLife.Commons;

namespace ManageLife.Models
{
    public class ChangeLanguageResult
    {
        public string? ReturnUrl { get; set; }
        public string LanguageCode { get; set; } = LanguageConst.DEFAULT_LANGUAGE;
    }
}
