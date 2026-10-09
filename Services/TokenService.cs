using LinqKit;
using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Contexts;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ManageLife.Services
{
    public class TokenService : ServiceBase<TokenService>, ITokenService
    {
        public const int RefreshTokenExpiryDays = 7;
        /// <summary>Token vừa bị thay khi làm mới song song vẫn được dùng lại trong khoảng này (tránh đăng xuất oan).</summary>
        private const int RefreshReuseGraceSeconds = 30;
        private const string InvalidSessionMessage = "Phiên đăng nhập không hợp lệ hoặc đã hết hạn";

        private readonly JwtOptions _jwt;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRefreshTokenRepository _refreshRepo;
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IUnitOfWork _uow;
        private readonly ICacheService _cache;
        private readonly ISettingContext _settingContext;

        public TokenService(
            IUserRefreshTokenRepository refreshRepo,
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IUserRoleRepository userRoleRepo,
            IOptions<JwtOptions> jwtOptions,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork uow,
            ICacheService cache,
            ISettingContext settingContext,
            IAppLogger<TokenService> logger,
            IUserContext userContext) : base(logger, userContext)
        {
            _jwt = jwtOptions.Value;
            _httpContextAccessor = httpContextAccessor;
            _refreshRepo = refreshRepo;
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _uow = uow;
            _cache = cache;
            _settingContext = settingContext;
        }

        #region Access Token
        public string GenerateAccessToken(string userId, string username, string securityStamp, string sessionId, IEnumerable<string> roles)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, username),
                new(JwtConst.SECURITY_STAMP, securityStamp),
                new(JwtConst.SESSION_ID, sessionId)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTimeHelper.UtcNow().AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateAccessToken(string? token)
        {
            if (token.IsEmpty())
                return null;

            var handler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwt.Issuer,
                ValidAudience = _jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
                ClockSkew = TimeSpan.Zero,
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };

            try
            {
                return handler.ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> ValidateSecurityStampAsync(ClaimsPrincipal principal, CancellationToken ct = default)
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var stampInToken = principal.FindFirstValue(JwtConst.SECURITY_STAMP);

            if (userId.IsEmpty() || stampInToken.IsEmpty())
                return false;

            //NOTE: Phiên đã bị đăng xuất (từ thiết bị khác) thì chặn ngay, không chờ access token hết hạn
            var sessionId = principal.FindFirstValue(JwtConst.SESSION_ID);
            if (sessionId.IsNotEmpty() && await _cache.TryGetValueAsync<string>(CacheSettings.RevokedSession(sessionId!)) != null)
                return false;

            var cacheItem = CacheSettings.SecurityStamp(userId!);
            var cachedStamp = await _cache.TryGetValueAsync<string>(cacheItem);

            if (cachedStamp != null)
                return string.Equals(cachedStamp, stampInToken, StringComparison.Ordinal);

            // Cache miss → query DB và cache lại
            var user = await _userRepo.GetAsync(userId!);
            if (user == null || user.IsDeleted || !user.IsActive || user.SecurityStamp.IsEmpty())
                return false;

            await _cache.SetAsync(user.SecurityStamp!, cacheItem);
            return string.Equals(user.SecurityStamp, stampInToken, StringComparison.Ordinal);
        }

        public async Task InvalidateSecurityStampCacheAsync(string userId, CancellationToken ct = default)
        {
            await _cache.RemoveAsync(CacheSettings.SecurityStamp(userId));
        }
        #endregion

        #region Refresh Token
        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<Result<AuthTokenModel>> RefreshTokenAsync(string? refreshToken, CancellationToken ct = default)
        {
            try
            {
                if (refreshToken.IsEmpty())
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, InvalidSessionMessage);

                var now = DateTimeHelper.UtcNow();
                var tokenHash = HashRefreshToken(refreshToken!);
                var tokenEntity = await _refreshRepo.Query(true)
                    .FirstOrDefaultAsync(r => r.RefreshToken == tokenHash && r.ExpiryTime > now, ct);

                if (tokenEntity == null)
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, InvalidSessionMessage);

                var user = await _userRepo.GetAsync(tokenEntity.UserId, ct);

                if (user == null || user.IsDeleted || !user.IsActive)
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, InvalidSessionMessage);

                // Phiên vừa bị đăng xuất từ thiết bị khác thì không cho làm mới
                if (tokenEntity.SessionId.IsNotEmpty() &&
                    await _cache.TryGetValueAsync<string>(CacheSettings.RevokedSession(tokenEntity.SessionId)) != null)
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, InvalidSessionMessage);

                await _uow.BeginTransactionAsync(ct);

                //NOTE: Thu hồi token cũ có điều kiện: 2 lần làm mới song song (hoặc vừa bị đăng xuất phiên) thì chỉ 1 bên thắng
                var claimed = tokenEntity.IsRevoked ? 0 : await _refreshRepo.Query()
                    .Where(x => x.Id == tokenEntity.Id && !x.IsRevoked)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.IsRevoked, true)
                        .SetProperty(x => x.RevokedAt, now), ct);
                if (claimed != 1 && !await IsJustRotatedAsync(tokenEntity, now, ct))
                {
                    await _uow.RollbackAsync(ct);
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, InvalidSessionMessage);
                }

                // Xoay vòng token nhưng giữ nguyên phiên (thiết bị, lúc đăng nhập)
                var issued = await IssueTokensAsync(user, ct, tokenEntity);
                if (!issued.IsOk())
                {
                    await _uow.RollbackAsync(ct);
                    return issued;
                }

                await _uow.CommitAsync(ct);
                return issued;
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                var msg = "Đã có lỗi xảy ra khi làm mới phiên đăng nhập";
                _logger.Error(ex, msg);
                return Result.Exception<AuthTokenModel>(msg, ex);
            }
        }

        /// <summary>
        /// Tạo phiên mới: dọn token hết hạn/đã revoke của user, lưu refresh token mới và cấp access token.
        /// Chạy trong transaction của caller (nếu có). Không đụng cookie — caller tự quyết (web set cookie, mobile thì không).
        /// </summary>
        public Task<Result<AuthTokenModel>> IssueTokensAsync(UserEntity user, CancellationToken ct = default)
            => IssueTokensAsync(user, ct, null);

        /// <summary>
        /// Token vừa bị thay bởi 1 lần làm mới song song (web bắn nhiều request khi access token vừa hết hạn):
        /// bị thu hồi trong RefreshReuseGraceSeconds và phiên vẫn còn token đang dùng. Phiên bị đăng xuất thì
        /// mọi token đều đã thu hồi nên không lọt qua đây.
        /// Đánh đổi có chủ ý: trong 30 giây đó token cũ dùng lại được nhiều lần (để mọi request song song đều qua);
        /// lệnh đăng xuất phiên vẫn thu hồi hết các token sinh ra.
        /// </summary>
        private async Task<bool> IsJustRotatedAsync(UserRefreshTokenEntity token, DateTime now, CancellationToken ct)
        {
            if (token.SessionId.IsEmpty()) return false;
            var re = await _refreshRepo.Query(true).FirstOrDefaultAsync(x => x.Id == token.Id, ct);
            if (re?.RevokedAt is not { } revokedAt || revokedAt < now.AddSeconds(-RefreshReuseGraceSeconds)) return false;

            //NOTE: UPDATE (khoá dòng) thay vì SELECT: lệnh đăng xuất phiên chạy song song phải chờ hoặc làm nhánh này thấy 0 dòng,
            // không để chèn token mới vào một phiên vừa bị thu hồi. Đồng thời cập nhật lần hoạt động của phiên
            var live = await _refreshRepo.Query()
                .Where(x => x.UserId == token.UserId && x.SessionId == token.SessionId && !x.IsRevoked && x.ExpiryTime > now)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IssuedAt, now), ct);
            return live > 0;
        }

        /// <param name="previous">Token đang được làm mới: giữ phiên của nó; null = đăng nhập mới (phiên mới).</param>
        private async Task<Result<AuthTokenModel>> IssueTokensAsync(UserEntity user, CancellationToken ct, UserRefreshTokenEntity? previous)
        {
            //NOTE: Chỉ dọn token cũ khi đăng nhập mới; lúc xoay vòng thì giữ token vừa thu hồi cho thời gian ân hạn
            // (job cron dọn hằng ngày)
            if (previous == null)
            {
                var cleanupResult = await CleanupRefreshTokensAsync(user.Id, ct: ct);
                if (!cleanupResult.IsOk())
                    return Result.Error<AuthTokenModel>(Result.DATA_NOT_DELETE.Code, "Không thể dọn dẹp token cũ");
            }

            var now = DateTimeHelper.UtcNow();
            var sessionId = previous?.SessionId.IsNotEmpty() == true ? previous.SessionId : IdHelper.NewId();
            var refreshToken = GenerateRefreshToken();
            var inserted = await _refreshRepo.InsertAsync(new UserRefreshTokenEntity
            {
                Id = IdHelper.NewId(),
                UserId = user.Id,
                //NOTE: Chỉ lưu hash: lộ DB cũng không dùng được token
                RefreshToken = HashRefreshToken(refreshToken),
                ExpiryTime = now.AddDays(RefreshTokenExpiryDays),
                SessionId = sessionId,
                DeviceName = previous != null ? previous.DeviceName : DescribeDevice(),
                SessionStartedAt = previous?.SessionStartedAt ?? now,
                IssuedAt = now
            }, ct);
            if (!inserted)
                return Result.Error<AuthTokenModel>(Result.DATA_NOT_CREATE.Code, "Không thể tạo phiên đăng nhập");

            // Trong transaction repo không tự SaveChanges: flush trước khi query role để thấy cả UserRole
            // vừa insert cùng transaction (vd đăng ký), nếu không token sẽ thiếu role claim.
            await _uow.SaveChangesAsync(ct);

            var roles = await _userRoleRepo.Query(true)
                .Where(ur => ur.UserId == user.Id)
                .Join(_roleRepo.Query(true), ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync(ct);

            var accessToken = GenerateAccessToken(user.Id, user.UserName, user.SecurityStamp!, sessionId, roles);
            return Result.Ok(new AuthTokenModel { AccessToken = accessToken, RefreshToken = refreshToken });
        }
        #endregion

        #region Sessions
        public string HashRefreshToken(string refreshToken)
            => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

        public async Task<Result<List<AccountSessionModel>>> GetSessionsAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<List<AccountSessionModel>>(Result.DATA_INVALID.Code, InvalidSessionMessage);
                var currentSessionId = CurrentSessionId();
                var now = DateTimeHelper.UtcNow();

                var tokens = await _refreshRepo.Query(true)
                    .Where(x => x.UserId == userId && !x.IsRevoked && x.ExpiryTime > now)
                    .ToListAsync(ct);

                var sessions = tokens
                    .GroupBy(x => x.SessionId)
                    .Select(g => new AccountSessionModel
                    {
                        SessionId = g.Key,
                        DeviceName = g.OrderByDescending(x => x.IssuedAt).First().DeviceName,
                        StartedAt = DateTime.SpecifyKind(g.Min(x => x.SessionStartedAt), DateTimeKind.Utc),
                        LastActiveAt = DateTime.SpecifyKind(g.Max(x => x.IssuedAt), DateTimeKind.Utc),
                        IsCurrent = g.Key == currentSessionId
                    })
                    .OrderByDescending(x => x.IsCurrent).ThenByDescending(x => x.LastActiveAt)
                    .ToList();

                return Result.Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi lấy danh sách phiên đăng nhập");
                return Result.Exception<List<AccountSessionModel>>("Có lỗi xảy ra khi lấy danh sách thiết bị", ex);
            }
        }

        public async Task<Result> RevokeSessionAsync(string sessionId, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty() || sessionId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, InvalidSessionMessage);

                // Chỉ phiên của chính người dùng
                // Chỉ phiên của chính người dùng; không tự đăng xuất phiên đang dùng qua đây (dùng Đăng xuất)
                if (sessionId == CurrentSessionId())
                    return Result.Error(Result.DATA_INVALID.Code, "Đây là thiết bị đang dùng, hãy dùng Đăng xuất");
                var revoked = await RevokeSessionsAsync(userId!, new[] { sessionId }, ct);
                if (revoked == 0) return Result.Error(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy phiên đăng nhập");
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi đăng xuất phiên");
                return Result.Exception("Có lỗi xảy ra khi đăng xuất thiết bị", ex);
            }
        }

        public async Task<Result> RevokeOtherSessionsAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                var currentSessionId = CurrentSessionId();
                if (userId.IsEmpty() || currentSessionId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, InvalidSessionMessage);

                var others = await _refreshRepo.Query(true)
                    .Where(x => x.UserId == userId && x.SessionId != currentSessionId && !x.IsRevoked)
                    .Select(x => x.SessionId)
                    .Distinct()
                    .ToListAsync(ct);
                if (others.Count == 0) return Result.Ok();

                // Thu hồi đúng các phiên đã liệt kê (phiên đăng nhập mới trong lúc đó không bị đụng)
                await RevokeSessionsAsync(userId!, others, ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi đăng xuất các phiên khác");
                return Result.Exception("Có lỗi xảy ra khi đăng xuất các thiết bị khác", ex);
            }
        }

        /// <summary>Thu hồi mọi token còn hiệu lực của các phiên (của chính user) và chặn access token của chúng.</summary>
        public async Task<int> RevokeSessionsAsync(string userId, IReadOnlyCollection<string> sessionIds, CancellationToken ct = default)
        {
            var now = DateTimeHelper.UtcNow();
            var revoked = await _refreshRepo.Query()
                .Where(x => x.UserId == userId && sessionIds.Contains(x.SessionId) && !x.IsRevoked)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRevoked, true).SetProperty(x => x.RevokedAt, now), ct);
            if (revoked > 0) await MarkSessionsRevokedAsync(sessionIds);
            return revoked;
        }

        public async Task MarkSessionsRevokedAsync(IEnumerable<string> sessionIds)
        {
            foreach (var sessionId in sessionIds.Where(x => x.IsNotEmpty()).Distinct())
            {
                var cacheItem = CacheSettings.RevokedSession(sessionId);
                await _cache.SetAsync("1", cacheItem);
                //NOTE: CacheService nuốt lỗi khi ghi: refresh token đã thu hồi, nhưng access token của phiên còn dùng được tới khi hết hạn (≤ 60 phút)
                if (await _cache.TryGetValueAsync<string>(cacheItem) == null)
                    _logger.Error(new InvalidOperationException("Ghi cache phiên bị thu hồi thất bại"),
                        "Không chặn ngay được access token của phiên {sessionId}", sessionId);
            }
        }

        /// <summary>Bỏ ký tự điều khiển, gộp khoảng trắng, tối đa 100 ký tự (không cắt giữa emoji).</summary>
        private static string CleanDeviceName(string value)
        {
            // Bỏ ký tự điều khiển + ký tự định dạng ẩn (bidi, zero-width) + ngắt dòng/đoạn Unicode
            var visible = value.Where(c => !char.IsControl(c) && CharUnicodeInfo.GetUnicodeCategory(c) is not
                (UnicodeCategory.Format or UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator));
            var cleaned = string.Join(' ', new string(visible.ToArray()).Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (cleaned.Length <= 100) return cleaned;
            var cut = char.IsHighSurrogate(cleaned[99]) ? 99 : 100;
            return cleaned[..cut];
        }

        private static string SafeUnescape(string value)
        {
            try { return Uri.UnescapeDataString(value); }
            catch (UriFormatException) { return value; }
        }

        private string? CurrentSessionId()
            => _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtConst.SESSION_ID);

        /// <summary>Tên thiết bị: header X-Device-Name (app gửi), không có thì đoán từ User-Agent.</summary>
        private string DescribeDevice()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            //NOTE: App gửi tên đã encodeURIComponent (header chỉ nhận ASCII, tên máy có thể có dấu)
            var header = request?.Headers["X-Device-Name"].ToString();
            var deviceName = header.IsNotEmpty() ? CleanDeviceName(SafeUnescape(header!)) : null;
            if (deviceName.IsNotEmpty()) return deviceName!;

            var userAgent = request?.Headers.UserAgent.ToString() ?? "";
            var browser = userAgent.Contains("Edg/") ? "Edge"
                : userAgent.Contains("Chrome/") ? "Chrome"
                : userAgent.Contains("Firefox/") ? "Firefox"
                : userAgent.Contains("Safari/") ? "Safari"
                : null;
            var os = userAgent.Contains("Windows") ? "Windows"
                : userAgent.Contains("Android") ? "Android"
                : userAgent.Contains("iPhone") || userAgent.Contains("iPad") ? "iOS"
                : userAgent.Contains("Mac OS X") ? "macOS"
                : userAgent.Contains("Linux") ? "Linux"
                : null;
            var name = string.Join(" · ", new[] { browser, os }.Where(x => x != null));
            return name.IsNotEmpty() ? name : "Thiết bị không rõ";
        }
        #endregion

        #region Cookie Management
        public async Task SetTokensCookieAsync(string accessToken, string refreshToken)
        {
            var context = _httpContextAccessor.HttpContext!;

            var sessionTimeoutMinutes = await _settingContext.GetIntAsync(SettingKeys.Security.SessionTimeoutMinutes, 60);

            context.Response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeHelper.UtcNow().AddMinutes(sessionTimeoutMinutes)
            });

            context.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeHelper.UtcNow().AddDays(RefreshTokenExpiryDays)
            });
        }

        public void ClearTokensCookie()
        {
            var context = _httpContextAccessor.HttpContext!;
            context.Response.Cookies.Delete("accessToken");
            context.Response.Cookies.Delete("refreshToken");
        }
        #endregion

        public async Task<Result> CleanupRefreshTokensAsync(string? userId = null, IUnitOfWork? uow = null, CancellationToken ct = default)
        {
            try
            {
                // Giữ token vừa thu hồi trong thời gian ân hạn (request làm mới song song còn cần tới nó)
                var graceFrom = DateTimeHelper.UtcNow().AddSeconds(-RefreshReuseGraceSeconds);
                var predicate = PredicateBuilder.New<UserRefreshTokenEntity>(x => x.ExpiryTime <= DateTimeHelper.UtcNow()
                    || (x.IsRevoked && (x.RevokedAt == null || x.RevokedAt < graceFrom)));

                if (userId.IsNotEmpty())
                    predicate = predicate.And(x => x.UserId == userId);

                await _refreshRepo.Query().Where(predicate).ExecuteDeleteAsync(ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, TranslationKey.Common.Message.SystemError);
                return Result.Exception(TranslationKey.Common.Message.SystemError, ex);
            }
        }
    }
}
