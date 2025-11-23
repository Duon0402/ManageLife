using ManageLife.Commons;

namespace ManageLife.Models
{
    public class SettingModel
    {
        public string Id { get; set; } = null!;
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public SettingType Type { get; set; }
    }
}
