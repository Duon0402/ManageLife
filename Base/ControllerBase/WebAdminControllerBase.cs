using ManageLife.Data;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Base
{
    [Route("Admin/[controller]")]
    public class WebAdminControllerBase : Controller
    {
        protected readonly AppDbContext _context;
        protected readonly ILogger? _logger;

        public WebAdminControllerBase(AppDbContext context, ILogger? logger = null)
        {
            _context = context;
            _logger = logger;
        }

        protected new ViewResult View(string? viewName = null, object? model = null)
        {
            var controllerName = ControllerContext.RouteData.Values["controller"]?.ToString();
            viewName ??= "Index";

            var fullPath = $"~/Views/Admin/{controllerName}/{viewName}.cshtml";
            return base.View(fullPath, model);
        }
    }
}
