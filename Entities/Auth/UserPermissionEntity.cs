namespace ManageLife.Entities
{
    public class UserPermissionEntity
    {
        public string RoleId { get; set; } = null!;
        public RoleEntity? Role { get; set; }

        public string PermissionId { get; set; } = null!;
        public PermissionEntity? Permission { get; set; }
    }
}
