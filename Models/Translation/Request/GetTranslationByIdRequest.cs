using ManageLife.Core;

namespace ManageLife.Models
{
    public class GetTranslationByIdRequest : IValidatableRequest
    {
        public string Id { get; set; } = null!;
    }
}
