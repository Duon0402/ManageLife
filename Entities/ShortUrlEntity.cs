using ManageLife.Core;

namespace ManageLife.Entities
{
    public class ShortUrlEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string Code { get; set; } = default!;
        public string OriginalUrl { get; set; } = default!;
        public string? Title { get; set; }
        public int ClickCount { get; set; }
        public DateTime? ExpireAt { get; set; }
        public string OwnerId { get; set; } = default!;
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
