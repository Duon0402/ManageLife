using System.Security.Claims;

namespace ManageLife.Contexts
{
    public static class UserContext
    {
        private static IHttpContextAccessor? _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static HttpContext? Current => _httpContextAccessor?.HttpContext;

        public static ClaimsPrincipal? User => Current?.User;

        public static string? GetUserId() => User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public static string? GetUserName() => User?.FindFirstValue(ClaimTypes.Name);

        public static IEnumerable<string> GetUserRoles() => User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();

        public static bool HasRole(string role) => User?.IsInRole(role) ?? false;
    }
}
