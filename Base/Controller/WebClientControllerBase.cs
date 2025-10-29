using ManageLife.Data;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Base
{
    public class WebClientControllerBase : WebControllerBase
    {
        public WebClientControllerBase(AppDbContext context, ILogger? logger = null) : base(context, logger)
        {
        }

        [NonAction]
        public override ViewResult View() => base.View(GetClientViewPath());

        [NonAction]
        public override ViewResult View(string? viewName) => base.View(GetClientViewPath(viewName));

        [NonAction]
        public override ViewResult View(object? viewModel) => base.View(GetClientViewPath(), viewModel);

        [NonAction]
        public override ViewResult View(string? viewName, object? viewModel)
        {
            if (!string.IsNullOrEmpty(viewName) && viewName.StartsWith("~/Views/Client"))
                return base.View(viewName, viewModel);

            return base.View(GetClientViewPath(viewName), viewModel);
        }

        [NonAction]
        private string GetClientViewPath(string? viewName = null)
        {
            var controllerName = GetType().Name.Replace("Controller", "");
            if (string.IsNullOrEmpty(viewName)) viewName = "Index";
            return $"~/Views/Client/{controllerName}/{viewName}.cshtml";
        }
    }
}
