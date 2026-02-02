namespace ManageLife.Entities
{
    public class UserRoleEntity
    {
        public string UserId { get; set; } = null!;
        public UserEntity User { get; set; } = null!;

        public string RoleId { get; set; } = null!;
        public RoleEntity Role { get; set; } = null!;
    }
}
