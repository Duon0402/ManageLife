using ManageLife.Base;

namespace ManageLife.Entities
{
    public class RoleEntity : EntityBase, CanCreate, ICanUpdate, ISoftDelete
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
