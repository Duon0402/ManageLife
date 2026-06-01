using ManageLife.Core;

namespace ManageLife.Models
{
    public class GetSettingByKeyRequest : IValidatableRequest
    {
        public string Key { get; set; } = null!;
    }
}
