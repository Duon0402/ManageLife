using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Models
{
    public class GetListSettingsRequest : IValidatableRequest
    {
        public SettingType Type { get; set; }
    }
}
