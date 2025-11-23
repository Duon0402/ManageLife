using ManageLife.Commons;

namespace ManageLife.Models
{
    public class CreateSettingRequest
    {
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public SettingType Type { get; set; }
    }
}
