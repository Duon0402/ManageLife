using ManageLife.Extentions;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

public class PermissionAttribute : ActionFilterAttribute
{
    public string Permission { get; }

    private static readonly MemoryCache _permissionCache = new(new MemoryCacheOptions());

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
        var cacheKey = $"permissions_{userId}";

        var permissions = _permissionCache.GetOrCreate(cacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            var service = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
            var result = service.GetListPermissionsByUserIdAsync(new GetListPermissionsByUserIdRequest { UserId = userId }).Result;
            return result.IsOk() ? result.Data?.Select(p => p.Code).ToHashSet() ?? new HashSet<string>() : new HashSet<string>();
        });

        if (!permissions.Contains(permissionCode))
        {
            HandleForbidden(context, isAjax);
            return;
        }

        await next();
    }

    private static string BuildPermissionCode(RouteData routeData, string permission)
    {
        var area = routeData.Values.TryGetValue("area", out var a) ? a?.ToString() : "Default";
        var controller = routeData.Values.TryGetValue("controller", out var c) ? c?.ToString() : throw new InvalidOperationException("Controller missing in RouteData");
        return $"{area}.{controller}.{permission}";
    }

    private static void HandleUnauthorized(ActionExecutingContext context, bool isAjax)
    {
        if (isAjax)
            context.Result = new JsonResult(new { code = "401", message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
        else
            context.Result = new RedirectResult($"/Auth/Login?returnUrl={Uri.EscapeDataString(context.HttpContext.Request.Path + context.HttpContext.Request.QueryString)}");
    }

    private static void HandleForbidden(ActionExecutingContext context, bool isAjax)
    {
        if (isAjax)
            context.Result = new JsonResult(new { code = "403", message = "Forbidden" }) { StatusCode = StatusCodes.Status403Forbidden };
        else
            context.Result = new RedirectResult("/Auth/AccessDenied");
    }
}
