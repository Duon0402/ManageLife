using ManageLife.Extentions;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class PermissionAttribute : ActionFilterAttribute
{
    public string Permission { get; }

    public PermissionAttribute(string permission) => Permission = permission;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = context.HttpContext.User.GetUserId();
        var isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (string.IsNullOrEmpty(userId))
        {
            HandleUnauthorized(context, isAjax);
            return;
        }

        var permissionCode = BuildPermissionCode(context.RouteData, Permission);

        var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>()
            ?? throw new InvalidOperationException("IPermissionService is not registered in DI.");

        var result = await permissionService.GetListPermissionsByUserIdAsync(
            new GetListPermissionsByUserIdRequest { UserId = userId }
        );

        if (result.IsError() || result.Data?.Any(p => p.Code == permissionCode) != true)
        {
            HandleForbidden(context, isAjax);
            return;
        }

        await next();
    }

    private static string BuildPermissionCode(RouteData routeData, string permission)
    {
        var values = routeData.Values;

        var area = values.TryGetValue("area", out var areaObj) && !string.IsNullOrEmpty(areaObj?.ToString())
            ? areaObj!.ToString()
            : "Default";

        if (!values.TryGetValue("controller", out var controllerObj) || string.IsNullOrEmpty(controllerObj?.ToString()))
            throw new InvalidOperationException("RouteData does not contain a valid 'controller'.");

        var controller = controllerObj!.ToString();
        return $"{area}.{controller}.{permission}";
    }

    private static void HandleUnauthorized(ActionExecutingContext context, bool isAjax)
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
    }

    private static void HandleForbidden(ActionExecutingContext context, bool isAjax)
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
    }
}
