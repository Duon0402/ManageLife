using ManageLife.Core;

namespace ManageLife.Entities
{
    public class ShortUrlClickEntity : EntityBase, ICanCreate
    {
        public string ShortUrlId { get; set; } = default!;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Referer { get; set; }
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
    }
}
