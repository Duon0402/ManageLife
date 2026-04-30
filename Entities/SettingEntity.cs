using ManageLife.Core;
using ManageLife.Commons;

namespace ManageLife.Entities
{
    public class SettingEntity : EntityBase
    {
        public string Key { get; set; } = default!;
        public string Value { get; set; } = default!;
        public SettingType Type { get; set; }
    }
}
