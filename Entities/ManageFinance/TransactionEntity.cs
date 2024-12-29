using ManageLife.Base;

namespace ManageLife.Entities
{
    public class TransactionEntity : EntityBase, CanCreate, CanUpdate, CanDelete
    {
        public int TransitionType { get; set; }
        public string TransactionCategoryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
