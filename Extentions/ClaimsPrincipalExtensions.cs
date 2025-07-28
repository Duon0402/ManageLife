using System.Security.Claims;

namespace ManageLife.Extentions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public static string? GetUserName(this ClaimsPrincipal user)
        {
            return user?.FindFirstValue(ClaimTypes.Name);
        }

        public static IEnumerable<string> GetUserRoles(this ClaimsPrincipal user)
        {
            return user?.FindAll(ClaimTypes.Role).Select(r => r.Value) ?? Enumerable.Empty<string>();
        }

        public static bool IsInRole(this ClaimsPrincipal user, string role)
        {
            return user?.IsInRole(role) ?? false;
        }
    }
}
