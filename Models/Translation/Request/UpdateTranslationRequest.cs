using ManageLife.Core;

namespace ManageLife.Models
{
    public class UpdateTranslationRequest : IValidatableRequest
    {
        public string Id { get; set; } = null!;
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string LanguageId { get; set; } = null!;
    }
}
