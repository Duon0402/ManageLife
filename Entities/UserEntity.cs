using ManageLife.Base;

namespace ManageLife.Entities
{
    public class UserEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string UserName { get; set; } = default!;
        public string? Email { get; set; }
        public string HashPassword { get; set; } = default!;
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public string? SecurityStamp { get; set; }
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
