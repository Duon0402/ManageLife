using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

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

        private static string GetUrl(Expression<Action<TController>> action)
        {
            if (action.Body is MethodCallExpression methodCall)
            {
                var actionName = methodCall.Method.Name;
                var controllerName = typeof(TController).Name.Replace("Controller", "");
                return $"/Admin/{controllerName}/{actionName}".ToLower();
            }
            return "#";
        }
    }
}
