using ManageLife.Base;
using ManageLife.Commons;

namespace ManageLife.Entities
{
    public class SettingEntity : EntityBase
    {
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public SettingType Type { get; set; }
    }
}
