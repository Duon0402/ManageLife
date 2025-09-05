using ManageLife.Extentions;
using ManageLife.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ManageLife.Base
{
    public class AuthorizeCustomAttribute : ActionFilterAttribute
    {
        private readonly string _permission;

        public AuthorizeCustomAttribute(string permission) => _permission = permission;

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var routeData = context.RouteData.Values;
            var area = routeData.TryGetValue("area", out var areaObj) && !string.IsNullOrEmpty(areaObj?.ToString())
                ? areaObj!.ToString()
                : "Default";

            if (!routeData.TryGetValue("controller", out var controllerObj) || string.IsNullOrEmpty(controllerObj?.ToString()))
                throw new InvalidOperationException("RouteData does not contain a valid 'controller'.");

            var controller = controllerObj!.ToString();
            var permissionCode = $"{area}.{controller}.{_permission}";
            var userId = context.HttpContext.User.GetUserId();
            var isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (string.IsNullOrEmpty(userId))
            {
                if (isAjax)
                {
                    context.Result = new JsonResult(new { code = "401", message = "Unauthorized" })
                    { StatusCode = StatusCodes.Status401Unauthorized };
                }
                else
                {
                    var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                    context.Result = new RedirectResult($"/Auth/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                }
                return;
            }

            var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>()
                ?? throw new InvalidOperationException("IPermissionService is not registered in DI.");

            var result = await permissionService.GetListPermissionsByUserIdAsync(
                new Models.GetListPermissionsByUserIdRequest { UserId = userId }
            );

            if (result.IsError() || result.Data?.Any(p => p.Code == permissionCode) != true)
            {
                if (isAjax)
                {
                    context.Result = new JsonResult(new { code = "403", message = "Forbidden: You don't have permission" })
                    { StatusCode = StatusCodes.Status403Forbidden };
                }
                else
                {
                    context.Result = new RedirectResult("/Auth/AccessDenied");
                }
                return;
            }

            await next();
        }
    }
}
