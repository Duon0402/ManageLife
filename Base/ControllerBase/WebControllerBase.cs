using ManageLife.Data;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Base
{
	public class WebControllerBase : Controller
	{
		private readonly AppDbContext _context;
		private readonly ILogger? _logger;

		public WebControllerBase(AppDbContext context, ILogger? logger = null)
		{
			_context = context;
			_logger = logger;
		}
	}
}
