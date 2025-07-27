using ManageLife.Base;

namespace ManageLife.Entities
{
    public class UserEntity : EntityBase
    {
        public string UserName { get; set; } = null!;
        public string HashPassword { get; set; } = null!;
        public string RoleId { get; set; } = null!;
    }
}