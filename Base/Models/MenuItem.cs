using ManageLife.Commons;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;

namespace ManageLife.Base
{
    public class MenuItem
    {
        // NOTE: Dùng [JsonConstructor] Serializer không chọn nhầm constructor của Group khi lấy từ Cache (tránh lỗi ArgumentException)
        [JsonConstructor]
        public MenuItem(string title, MenuItemType menuItemType, string? icon = null, List<MenuItem>? subItems = null, string? url = null, string? permissionCode = null)
        {
            Title = title;
            Url = url;
            Icon = icon;
            PermissionCode = permissionCode;
            MenuItemType = menuItemType;

            SubItems = subItems ?? new List<MenuItem>();
            if (menuItemType == MenuItemType.Group && SubItems.IsEmpty())
            {
                throw new ArgumentException("Group menu must contain at least one sub item.", nameof(subItems));
            }
        }

        public MenuItem(string title, string icon, List<MenuItem> subItems) : this(title, MenuItemType.Group, icon, subItems)
        {
        }

        public string Title { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public string? PermissionCode { get; set; }
        public List<MenuItem> SubItems { get; set; }
        public MenuItemType MenuItemType { get; set; }
        public bool HasSubItems => SubItems.IsNotEmpty();
    }

    public class MenuItem<TController> : MenuItem where TController : Controller
    {
        public MenuItem(string title, Expression<Action<TController>> action, string? icon = null)
            : base(title, MenuItemType.Link, icon, null, GetUrl(action), GetPermissionCode(action))
        {
        }

        public MenuItem(string title, Expression<Func<TController, Task<IActionResult>>> action, string? icon = null)
            : base(title, MenuItemType.Link, icon, null, GetUrl(action), GetPermissionCode(action))
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
