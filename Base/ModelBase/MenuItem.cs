using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Reflection;

namespace ManageLife.Base
{
    public class MenuItem
    {
        public MenuItem(string title, string? url = null, string? icon = null, List<MenuItem>? subItems = null)
        {
            Title = title;
            Url = url;
            Icon = icon;
            SubItems = subItems ?? new List<MenuItem>();
        }

        public string Title { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public List<MenuItem> SubItems { get; set; }

        public bool HasChildren => SubItems.Any();
    }

    public class MenuItem<TController> : MenuItem where TController : Controller
    {
        public MenuItem(string title, Expression<Action<TController>> action, string? icon = null, List<MenuItem>? subItems = null)
            : base(title, GetUrl(action), icon, subItems)
        {
        }

        public MenuItem(string title, Expression<Func<TController, Task<IActionResult>>> action, string? icon = null, List<MenuItem>? subItems = null)
            : base(title, GetUrl(action), icon, subItems)
        {
        }

        private static string GetUrl(LambdaExpression action)
        {
            if (action.Body is MethodCallExpression methodCall)
            {
                var actionName = methodCall.Method.Name;
                var controllerName = typeof(TController).Name.Replace("Controller", "");

                var areaAttr = typeof(TController).GetCustomAttribute<AreaAttribute>();
                var area = areaAttr?.RouteValue ?? "Admin";

                if (actionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
                {
                    return $"/{area}/{controllerName}".ToLower();
                }

                return $"/{area}/{controllerName}/{actionName}".ToLower();
            }
            return "#";
        }
    }
}
