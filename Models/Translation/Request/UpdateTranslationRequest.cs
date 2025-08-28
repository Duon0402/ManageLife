namespace ManageLife.Models
{
    public class UpdateTranslationRequest
    {
        public string Id { get; set; } = null!;
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string LanguageId { get; set; } = null!;
    }
}
