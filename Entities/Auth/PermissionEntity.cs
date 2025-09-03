using ManageLife.Base;

namespace ManageLife.Entities
{
    public class PermissionEntity : EntityBase, ICanCreate
    {
        public string Code { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }

        public string CreatedUser { get; set; } = null!;
        public DateTime CreatedTime { get; set; }

        public ICollection<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();
        public ICollection<UserPermissionEntity> UserPermissions { get; set; } = new List<UserPermissionEntity>();
    }
}
