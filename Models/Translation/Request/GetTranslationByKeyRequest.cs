namespace ManageLife.Models
{
    public class GetTranslationByKeyRequest
    {
        public string Key { get; set; } = null!;
        public string LanguageCode { get; set; } = null!;
    }
}
