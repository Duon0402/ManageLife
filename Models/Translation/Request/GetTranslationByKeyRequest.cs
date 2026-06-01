using ManageLife.Core;

namespace ManageLife.Models
{
    public class GetTranslationByKeyRequest : IValidatableRequest
    {
        public string Key { get; set; } = null!;
        public string LanguageCode { get; set; } = null!;
    }
}
