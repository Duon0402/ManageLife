using ManageLife.Core;

namespace ManageLife.Models
{
    public class DeleteTranslationRequest : IValidatableRequest
    {
        public string Id { get; set; } = null!;
    }
}
