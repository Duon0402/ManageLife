using ManageLife.Core;

namespace ManageLife.Models
{
    public class UpdateSettingRequest : IValidatableRequest
    {
        public string Id { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}
