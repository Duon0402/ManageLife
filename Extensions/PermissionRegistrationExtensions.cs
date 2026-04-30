using ManageLife.Core;
using ManageLife.Interfaces;
using System.Reflection;

namespace ManageLife.Extensions
{
    public static class PermissionRegistrationExtensions
    {
        public static async Task RegisterPermissionsAsync(this IServiceProvider services, Assembly assembly)
        {
            using var scope = services.CreateScope();
            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

            var permissions = PermissionScanner.ScanPermissions(assembly);

            var result = await permissionService.SyncPermissionsAsync(permissions);
            if (result.IsError())
            {
                throw new InvalidOperationException($"Permission sync failed: {result.Message}");
            }
        }
    }
}
