using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManageLife.Models
{
	public class TransactionViewModel
	{
		public TransactionViewModel()
		{
			ListTransactionTypes = new List<SelectListItem>();
		}

		public List<SelectListItem> ListTransactionTypes { get; set; }
	}
}
