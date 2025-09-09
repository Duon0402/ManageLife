using ManageLife.Base;
using System.Security.Claims;

namespace ManageLife.Interfaces
{
    public interface ITokenService
    {
        public string GenerateAccessToken(string userId, string username, IEnumerable<string> roles);
        public string GenerateRefreshToken();
        public ClaimsPrincipal? ValidateAccessToken(string? token);
        public void SetTokensCookie(string userId, string username, IEnumerable<string> roles, string refreshToken);
        public void ClearTokensCookie();
        public Task<Result> RefreshTokenAsync(string? refreshToken);
    }
}
