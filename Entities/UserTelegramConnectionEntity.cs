using ManageLife.Core;

namespace ManageLife.Entities
{
    public class UserTelegramConnectionEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public long ChatId { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
