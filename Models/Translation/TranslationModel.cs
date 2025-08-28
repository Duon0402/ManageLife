namespace ManageLife.Models
{
    public class TranslationModel
    {
        public string Id { get; set; } = null!;
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;

        public string LanguageId { get; set; } = null!;
        public string LanguageCode { get; set; } = null!;
        public string LanguageName { get; set; } = null!;
    }
}
