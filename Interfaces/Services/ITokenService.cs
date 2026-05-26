using ManageLife.Core;
using ManageLife.Models;
using System.Security.Claims;

namespace ManageLife.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(string userId, string username, string securityStamp, IEnumerable<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateAccessToken(string? token);
        void SetTokensCookie(string accessToken, string refreshToken);
        void ClearTokensCookie();
        Task<Result<AuthTokenModel>> RefreshTokenAsync(string? refreshToken, CancellationToken ct = default);
        Task<Result> CleanupRefreshTokensAsync(string? userId = null, IUnitOfWork? uow = null, CancellationToken ct = default);
        Task<bool> ValidateSecurityStampAsync(ClaimsPrincipal principal, CancellationToken ct = default);
        Task InvalidateSecurityStampCacheAsync(string userId, CancellationToken ct = default);
    }
}
