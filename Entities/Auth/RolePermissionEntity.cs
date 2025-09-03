namespace ManageLife.Entities
{
    public class RolePermissionEntity
    {
        public string RoleId { get; set; } = null!;
        public RoleEntity? Role { get; set; }

        public string PermissionId { get; set; } = null!;
        public PermissionEntity? Permission { get; set; }
    }
}
