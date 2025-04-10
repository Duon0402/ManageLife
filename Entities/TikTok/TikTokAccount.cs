using ManageLife.Base;

namespace ManageLife.Entities
{
    public class TikTokAccount : EntityBase, CanCreate, CanUpdate, SoftDelete
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public int Status { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
