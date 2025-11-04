using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Reflection;

namespace ManageLife.Base
{
    public class MenuItem
    {
        public MenuItem(string title, string? icon = null, List<MenuItem>? subItems = null, string? url = null, string? permissionCode = null)
        {
            Title = title;
            Url = url;
            Icon = icon;
            SubItems = subItems ?? new List<MenuItem>();
            PermissionCode = permissionCode;
        }

        public string Title { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public string? PermissionCode { get; set; }
        public List<MenuItem> SubItems { get; set; }

        public bool HasSubItems => SubItems.Any();
    }

    public class MenuItem<TController> : MenuItem where TController : Controller
    {
        public MenuItem(string title, Expression<Action<TController>> action, string? icon = null, List<MenuItem>? subItems = null)
            : base(title, icon, subItems, GetUrl(action), GetPermissionCode(action))
        {
        }

        public MenuItem(string title, Expression<Func<TController, Task<IActionResult>>> action, string? icon = null, List<MenuItem>? subItems = null)
            : base(title, icon, subItems, GetUrl(action), GetPermissionCode(action))
        {
        }

        private static string GetUrl(LambdaExpression action)
        {
            if (action.Body is not MethodCallExpression methodCall)
                return "#";

            var actionName = methodCall.Method.Name;
            var controllerName = typeof(TController).Name.Replace("Controller", "");

            var areaAttr = typeof(TController).GetCustomAttribute<AreaAttribute>();
            var area = areaAttr?.RouteValue ?? "Admin";

            if (actionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
                return $"/{area}/{controllerName}".ToLower();

            return $"/{area}/{controllerName}/{actionName}".ToLower();
        }

        private static string GetPermissionCode(LambdaExpression action)
        {
            if (action.Body is not MethodCallExpression methodCall)
                return string.Empty;

            var method = methodCall.Method;

            var hasAttr = method.GetCustomAttribute<AccessPagePermissionAttribute>() != null;
            if (!hasAttr)
                return string.Empty;

            var controllerName = typeof(TController).Name.Replace("Controller", "");
            var areaAttr = typeof(TController).GetCustomAttribute<AreaAttribute>();
            var area = areaAttr?.RouteValue ?? "Admin";

            return $"{area}.{controllerName}.{PermissionConst.AccessPage}";
        }
    }
}
