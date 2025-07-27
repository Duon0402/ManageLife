namespace ManageLife.Models
{
	public class TransactionModel
	{
		public string Id { get; set; }
		public int TransitionType { get; set; }
		public string TransactionCategoryId { get; set; }
		public decimal Amount { get; set; }
		public DateTime TransactionDate { get; set; }
		public string? Description { get; set; }
	}
}
