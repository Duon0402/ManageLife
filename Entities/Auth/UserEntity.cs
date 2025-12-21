using ManageLife.Base;

namespace ManageLife.Entities
{
    public class UserEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string HashPassword { get; set; } = null!;
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;

        public string SecurityStamp { get; set; } = null!;

        public string CreatedUser { get; set; } = null!;
        public DateTime CreatedTime { get; set; }
        string? ICanUpdate.UpdatedUser { get; set; }
        DateTime? ICanUpdate.UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
        public ICollection<UserPermissionEntity> UserPermissions { get; set; } = new List<UserPermissionEntity>();
    }
}
