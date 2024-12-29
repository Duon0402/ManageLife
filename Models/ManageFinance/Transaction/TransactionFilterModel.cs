namespace ManageLife.Models
{
    public class TransactionFilterModel
    {
        public int? TransitionType { get; set; }
        public string? TransactionCategoryId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
