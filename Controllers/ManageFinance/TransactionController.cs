using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Models;
using ManageLife.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManageLife.Controllers
{
	public class TransactionController : WebControllerBase
	{
		private readonly TransactionService _service;

		public TransactionController(AppDbContext context, ILogger? logger = null) : base(context, logger)
		{
			_service = new TransactionService(context);
		}

		public IActionResult Index()
		{
			var viewModel = new TransactionViewModel();

			viewModel.ListTransactionTypes = new List<SelectListItem>()
			{
				new SelectListItem(((int)TransitionType.Expense).ToString(), "Khoản chi"),
				new SelectListItem(((int)TransitionType.Income).ToString(), "Khoản thu")
			};

			return View(viewModel);
		}
	}
}
