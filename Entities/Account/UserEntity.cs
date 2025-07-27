using ManageLife.Base;

namespace ManageLife.Entities
{
    public class UserEntity : EntityBase
    {
        public string UserName { get; set; } = string.Empty;
        public string HashPassword { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
    }
}