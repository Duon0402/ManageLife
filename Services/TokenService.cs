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
using System.Security.Cryptography;
using System.Text;

namespace ManageLife.Services
{
    public class TokenService : ServiceBase<TokenService>, ITokenService
    {
        public const int RefreshTokenExpiryDays = 7;
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
        public string GenerateAccessToken(string userId, string username, string securityStamp, IEnumerable<string> roles)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, username),
                new(JwtConst.SECURITY_STAMP, securityStamp)
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

                var tokenEntity = await _refreshRepo.Query()
                    .FirstOrDefaultAsync(r => r.RefreshToken == refreshToken &&
                                              r.ExpiryTime > DateTimeHelper.UtcNow() &&
                                              r.IsRevoked == false, ct);

                if (tokenEntity == null)
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, InvalidSessionMessage);

                var user = await _userRepo.GetAsync(tokenEntity.UserId, ct);

                if (user == null || user.IsDeleted || !user.IsActive)
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, InvalidSessionMessage);

                await _uow.BeginTransactionAsync(ct);

                tokenEntity.IsRevoked = true;
                var updated = await _refreshRepo.UpdateAsync(tokenEntity, ct);
                if (!updated)
                {
                    await _uow.RollbackAsync(ct);
                    return Result.Error<AuthTokenModel>(Result.DATA_NOT_UPDATE.Code, "Không thể tạo phiên đăng nhập mới");
                }

                var issued = await IssueTokensAsync(user, ct);
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
        public async Task<Result<AuthTokenModel>> IssueTokensAsync(UserEntity user, CancellationToken ct = default)
        {
            var cleanupResult = await CleanupRefreshTokensAsync(user.Id, ct: ct);
            if (!cleanupResult.IsOk())
                return Result.Error<AuthTokenModel>(Result.DATA_NOT_DELETE.Code, "Không thể dọn dẹp token cũ");

            var refreshToken = GenerateRefreshToken();
            var inserted = await _refreshRepo.InsertAsync(new UserRefreshTokenEntity
            {
                Id = IdHelper.NewId(),
                UserId = user.Id,
                RefreshToken = refreshToken,
                ExpiryTime = DateTimeHelper.UtcNow().AddDays(RefreshTokenExpiryDays)
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

            var accessToken = GenerateAccessToken(user.Id, user.UserName, user.SecurityStamp!, roles);
            return Result.Ok(new AuthTokenModel { AccessToken = accessToken, RefreshToken = refreshToken });
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
                var predicate = PredicateBuilder.New<UserRefreshTokenEntity>(x => x.ExpiryTime <= DateTimeHelper.UtcNow() || x.IsRevoked);

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
