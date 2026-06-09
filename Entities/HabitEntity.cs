using ManageLife.Core;

namespace ManageLife.Entities
{
    public class HabitEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
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
