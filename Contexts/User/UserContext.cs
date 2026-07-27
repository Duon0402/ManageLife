using System.Security.Claims;

namespace ManageLife.Contexts
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _accessor;

        public UserContext(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private ClaimsPrincipal? User => _accessor.HttpContext?.User;

        public string? GetUserId() => User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public string? GetUserName() => User?.FindFirstValue(ClaimTypes.Name);

        public IEnumerable<string> GetUserRoles()
            => User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? [];

        public bool HasRole(string role) => User?.IsInRole(role) ?? false;
    }
}
