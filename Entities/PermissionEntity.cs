using ManageLife.Core;

namespace ManageLife.Entities
{
    public class PermissionEntity : EntityBase, ICanCreate, ICanUpdate
    {
        public string Code { get; set; } = default!;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
