namespace ManageLife.ViewModels
{
    public class AdminPermissionViewModel
    {
        public PermissionTargetType TargetType { get; set; }

        // User
        public string? UserId { get; set; }
        public string? UserName { get; set; }

        // Role
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
    }

    public enum PermissionTargetType
    {
        User = 1,
        Role = 2
    }
}
