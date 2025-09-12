using ManageLife.Base;
using ManageLife.Models;
using System.Security.Claims;

namespace ManageLife.Interfaces
{
    public interface ITokenService
    {
        public string GenerateAccessToken(string userId, string username, IEnumerable<string> roles);
        public string GenerateRefreshToken();
        public ClaimsPrincipal? ValidateAccessToken(string? token);
        public void SetTokensCookie(string accessToken, string refreshToken);
        public void ClearTokensCookie();
        public Task<Result<AuthTokenModel>> RefreshTokenAsync(string? refreshToken);
    }
}
