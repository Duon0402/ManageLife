using ManageLife.Data;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Base
{
    [Route("Admin/[controller]")]
    public class WebAdminControllerBase : WebControllerBase
    {
        public WebAdminControllerBase(AppDbContext context, ILogger? logger = null) : base(context, logger)
        {
        }

        [NonAction]
        public override ViewResult View() => base.View(GetAdminViewPath());

        [NonAction]
        public override ViewResult View(string? viewName) => base.View(GetAdminViewPath(viewName));

        [NonAction]
        public override ViewResult View(object? viewModel) => base.View(GetAdminViewPath(), viewModel);

        [NonAction]
        public override ViewResult View(string? viewName, object? viewModel)
        {
            if (!string.IsNullOrEmpty(viewName) && viewName.StartsWith("~/Views/Admin"))
                return base.View(viewName, viewModel);

            return base.View(GetAdminViewPath(viewName), viewModel);
        }

        [NonAction]
        private string GetAdminViewPath(string? viewName = null)
        {
            var controllerName = GetType().Name.Replace("Controller", "");
            if (string.IsNullOrEmpty(viewName)) viewName = "Index";
            return $"~/Views/Admin/{controllerName}/{viewName}.cshtml";
        }
    }
}
