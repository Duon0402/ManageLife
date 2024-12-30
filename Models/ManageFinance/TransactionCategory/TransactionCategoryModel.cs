namespace ManageLife.Models
{
    public class TransactionCategoryModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int TransactionType { get; set; }
    }
}
