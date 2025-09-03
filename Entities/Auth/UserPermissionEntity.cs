namespace ManageLife.Entities
{
    public class UserPermissionEntity
    {
        public string UserId { get; set; } = null!;
        public UserEntity? User { get; set; }

        public string PermissionId { get; set; } = null!;
        public PermissionEntity? Permission { get; set; }
    }
}
