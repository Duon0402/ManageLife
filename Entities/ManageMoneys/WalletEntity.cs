using ManageLife.Base;

namespace ManageLife.Entities
{
    public class WalletEntity : EntityBase, CanCreate, CanUpdate, CanDelete
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal TotalMoney { get; set; }

        public DateTime CreatedTime { get; set; }
        public string CreatedUser { get; set; }

        public DateTime UpdatedTime { get; set; }
        public string UpdatedUser { get; set; }

        public bool IsDeleted { get; set; }
        public string DeletedUser { get; set; }
        public DateTime DeletedTime { get; set; }
    }
}
