using ManageLife.Core;

namespace ManageLife.Models
{
    public class GetDictionaryTranslationByLanguageCodeRequest : IValidatableRequest
    {
        public string LanguageCode { get; set; } = null!;
    }
}
