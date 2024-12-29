using ManageLife.Base;

namespace ManageLife.Entities
{
    public class WalletEntity : EntityBase
    {
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public string CreatedUser { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
    }
}
