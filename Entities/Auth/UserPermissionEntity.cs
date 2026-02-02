using ManageLife.Base;

namespace ManageLife.Entities
{
    public class UserPermissionEntity
    {
        public UserPermissionStatus Status { get; set; }

        public string UserId { get; set; } = null!;
        public UserEntity? User { get; set; }

        public string PermissionId { get; set; } = null!;
        public PermissionEntity? Permission { get; set; }
    }
}
