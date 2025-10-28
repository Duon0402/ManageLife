using ManageLife.Base;
using ManageLife.Extensions;
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
        var httpContext = context.HttpContext;
        var userId = httpContext.User.GetUserId();

        if (userId.IsEmpty())
        {
            await base.OnActionExecutionAsync(context, next);
            return;
        }

        var isAjax = httpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        var permissionCode = BuildPermissionCode(context.RouteData, Permission);

        var service = httpContext.RequestServices.GetRequiredService<IPermissionService>();
        var result = await service.GetListPermissionsByUserIdAsync(new GetListPermissionsByUserIdRequest { UserId = userId });

        var permissions = result.IsOk()
            ? result.Data?.Select(p => p.Code).ToHashSet(StringComparer.OrdinalIgnoreCase)
              ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!permissions.Contains(permissionCode))
        {
            HandleForbidden(context, isAjax, permissionCode);
            return;
        }

        await next();
    }

    private static string BuildPermissionCode(RouteData routeData, string permission)
    {
        var area = routeData.Values.TryGetValue("area", out var a) ? a?.ToString() : "Default";
        var controller = routeData.Values.TryGetValue("controller", out var c)
                         ? c?.ToString()
                         : throw new InvalidOperationException("Controller missing in RouteData");

        return $"{area}.{controller}.{permission}";
    }

    private static void HandleForbidden(ActionExecutingContext context, bool isAjax, string permissionCode)
    {
        if (isAjax)
        {
            context.Result = new JsonResult(new { code = "403", message = "Forbidden" })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
        else
        {
            context.Result = new RedirectResult("/Auth/AccessDenied");
        }
    }
}
