using ManageLife.Base;

namespace ManageLife.Entities.ManageFinance
{
    public class TransactionCategoryEntity : EntityBase
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public int TransactionType { get; set; }
    }
}
