using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace ManageLife.Core
{
    public static class PermissionScanner
    {
        public static List<string> ScanPermissions(Assembly assembly)
        {
            var permissions = new List<string>();

            var controllers = assembly.GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t));

            foreach (var controller in controllers)
            {
                var area = controller.GetCustomAttribute<AreaAttribute>()?.RouteValue ?? "Default";

                var controllerName = controller.Name.Replace("Controller", "");

                var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                        .Where(m => m.DeclaringType == controller);

                foreach (var method in methods)
                {
                    var permAttributes = method.GetCustomAttributes<PermissionAttribute>();
                    foreach (var attr in permAttributes)
                    {
                        var code = $"{area}.{controllerName}.{attr.Permission}";
                        permissions.Add(code);
                    }
                }
            }

            return permissions.Distinct().ToList();
        }
    }
}
