using ManageLife.Core;

namespace ManageLife.Models
{
    public class GetListTranslationsRequest : IValidatableRequest
    {
        public string? LanguageCode { get; set; }
    }
}
