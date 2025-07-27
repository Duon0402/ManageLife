using ManageLife.Base;

namespace ManageLife.Entities
{
    public class RoleEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CreatedUser { get; set; } = "Unknown";
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
