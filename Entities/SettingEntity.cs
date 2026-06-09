using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Entities
{
    public class SettingEntity : EntityBase
    {
        public string Key { get; set; } = default!;
        public string Value { get; set; } = default!;
        public SettingType Type { get; set; }
        public string? Group { get; set; }
        public string? Description { get; set; }
    }
}
