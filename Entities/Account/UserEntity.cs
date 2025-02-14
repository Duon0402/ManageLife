using ManageLife.Base;

namespace ManageLife.Entities
{
    public class UserEntity : EntityBase
    {
        public string UserName { get; set; }
        public string HashPassword { get; set; }
        //public string RoleId { get; set; }
    }
}