using ManageLife.Base;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;

namespace ManageLife.Helpers
{
    public static class AjaxHelper
    {
        public static string GetActionUrl<TController>(Expression<Func<TController, object>> action) where TController : Controller
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var controllerType = typeof(TController);
            var controllerName = controllerType.Name.Replace("Controller", "");

            var areaAttr = controllerType.GetCustomAttribute<AreaAttribute>()
                           ?? controllerType.BaseType?.GetCustomAttribute<AreaAttribute>();

            var areaName = areaAttr?.RouteValue;

            if (action.Body is not MethodCallExpression methodCall)
                throw new ArgumentException("The expression must be a method call to a controller action.", nameof(action));

            var actionName = methodCall.Method.Name;

            var url = areaName.IsNotEmpty()
                ? $"/{areaName}/{controllerName}/{actionName}"
                : $"/{controllerName}/{actionName}";

            return url;
        }

        public static IHtmlContent AjaxGet(string url, object? queryParams = null)
        {
            var jsParams = queryParams != null ? JsonConvert.SerializeObject(queryParams) : "null";
            var script = $"ajaxService.get('{url}', {jsParams});";
            return new HtmlString(script);
        }

        public static IHtmlContent AjaxPost(string url, object? data = null)
        {
            var jsData = data != null ? JsonConvert.SerializeObject(data) : "null";
            var script = $"ajaxService.post('{url}', {jsData});";
            return new HtmlString(script);
        }

        public static IHtmlContent AjaxPut(string url, object? data = null)
        {
            var jsData = data != null ? JsonConvert.SerializeObject(data) : "null";
            var script = $"ajaxService.put('{url}', {jsData});";
            return new HtmlString(script);
        }

        public static IHtmlContent AjaxDelete(string url)
        {
            var script = $"ajaxService.delete('{url}');";
            return new HtmlString(script);
        }
    }
}
