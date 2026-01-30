namespace ManageLife.Models
{
    public class UserModel
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string HashPassword { get; set; } = null!;
        public string? FullName { get; set; }
        public bool IsActive { get; set; }

        public string SecurityStamp { get; set; } = null!;

        public string CreatedUser { get; set; } = null!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
