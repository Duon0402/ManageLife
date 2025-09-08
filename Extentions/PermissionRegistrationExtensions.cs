using ManageLife.Base;
using ManageLife.Interfaces;
using System.Reflection;

namespace ManageLife.Extentions
{
    public static class PermissionRegistrationExtensions
    {
        public static async Task RegisterPermissionsAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

            var permissions = PermissionScanner.ScanPermissions(Assembly.GetExecutingAssembly());

            await permissionService.SyncPermissionsAsync(permissions);
        }
    }
}
