using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Models
{
    public class CreateSettingRequest : IValidatableRequest
    {
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public SettingType Type { get; set; }
    }
}
