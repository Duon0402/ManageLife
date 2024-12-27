using ManageLife.Base;

namespace ManageLife.Entities
{
    public class WalletEntity : EntityBase
    {
        public string Name { get; set; }
        public decimal TotalMoney { get; set; }
        public string CreatedUser { get; set; } = "Admin";
        public DateTime CreatedTime { get; set; }
        public string UpdatedUser { get; set; } = "Admin";
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;
        public string DeletedUser { get; set; } = "Admin";
        public DateTime DeletedTime { get; set; } = DateTime.UtcNow;
    }
}
