using ManageLife.Entities;
using ManageLife.Core;
using ManageLife.Models;
using System.Security.Claims;

namespace ManageLife.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(string userId, string username, string securityStamp, string sessionId, IEnumerable<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateAccessToken(string? token);
        Task SetTokensCookieAsync(string accessToken, string refreshToken);
        void ClearTokensCookie();
        Task<Result<AuthTokenModel>> RefreshTokenAsync(string? refreshToken, CancellationToken ct = default);
        Task<Result<AuthTokenModel>> IssueTokensAsync(UserEntity user, CancellationToken ct = default);
        Task<Result> CleanupRefreshTokensAsync(string? userId = null, IUnitOfWork? uow = null, CancellationToken ct = default);
        Task<bool> ValidateSecurityStampAsync(ClaimsPrincipal principal, CancellationToken ct = default);
        Task InvalidateSecurityStampCacheAsync(string userId, CancellationToken ct = default);

        /// <summary>SHA-256 (base64) của refresh token — dạng lưu trong DB.</summary>
        string HashRefreshToken(string refreshToken);

        /// <summary>Các phiên đang hoạt động của người dùng hiện tại.</summary>
        Task<Result<List<AccountSessionModel>>> GetSessionsAsync(CancellationToken ct = default);

        /// <summary>Đăng xuất một phiên (thu hồi refresh token + chặn access token còn hạn).</summary>
        Task<Result> RevokeSessionAsync(string sessionId, CancellationToken ct = default);

        /// <summary>Đăng xuất mọi phiên khác phiên hiện tại.</summary>
        Task<Result> RevokeOtherSessionsAsync(CancellationToken ct = default);

        /// <summary>Thu hồi mọi token còn hiệu lực của các phiên (của chính user) và chặn access token của chúng.</summary>
        Task<int> RevokeSessionsAsync(string userId, IReadOnlyCollection<string> sessionIds, CancellationToken ct = default);

        /// <summary>Đánh dấu phiên đã đăng xuất để chặn access token còn hạn của nó.</summary>
        Task MarkSessionsRevokedAsync(IEnumerable<string> sessionIds);
    }
}
