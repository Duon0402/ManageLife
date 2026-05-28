using AutoMapper;
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
        private readonly JwtSettings _jwt;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRefreshTokenRepository _refreshRepo;
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IUnitOfWork _uow;
        private readonly ICacheService _cache;

        public TokenService(
            IMapper mapper,
            IUserRefreshTokenRepository refreshRepo,
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IUserRoleRepository userRoleRepo,
            IOptions<JwtSettings> jwtOptions,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork uow,
            ICacheService cache,
            IAppLogger<TokenService> logger,
            IUserContext userContext) : base(logger, userContext, mapper)
        {
            _jwt = jwtOptions.Value;
            _httpContextAccessor = httpContextAccessor;
            _refreshRepo = refreshRepo;
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _uow = uow;
            _cache = cache;
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
                {
                    ClearTokensCookie();
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, "Phiên đăng nhập không hợp lệ hoặc đã hết hạn");
                }

                var tokenEntity = await _refreshRepo.Query()
                    .FirstOrDefaultAsync(r => r.RefreshToken == refreshToken &&
                                              r.ExpiryTime > DateTimeHelper.UtcNow() &&
                                              r.IsRevoked == false, ct);

                if (tokenEntity == null)
                {
                    ClearTokensCookie();
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, "Phiên đăng nhập không hợp lệ hoặc đã hết hạn");
                }

                var user = await _userRepo.GetAsync(tokenEntity.UserId, ct);

                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    ClearTokensCookie();
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, "Phiên đăng nhập không hợp lệ hoặc đã hết hạn");
                }

                await _uow.BeginTransactionAsync(ct);

                tokenEntity.IsRevoked = true;
                var updated = await _refreshRepo.UpdateAsync(tokenEntity, ct);
                if (!updated)
                {
                    ClearTokensCookie();
                    return Result.Error<AuthTokenModel>(Result.DATA_NOT_UPDATE.Code, "Không thể tạo phiên đăng nhập mới");
                }

                var cleanupResult = await CleanupRefreshTokensAsync(tokenEntity.UserId, ct: ct);
                if (!cleanupResult.IsOk())
                    return Result.Error<AuthTokenModel>(Result.DATA_NOT_DELETE.Code, "Không thể dọn dẹp token cũ");

                var newRefreshToken = GenerateRefreshToken();
                var newRefreshEntity = new UserRefreshTokenEntity
                {
                    Id = IdHelper.NewId(),
                    UserId = tokenEntity.UserId,
                    RefreshToken = newRefreshToken,
                    ExpiryTime = DateTimeHelper.UtcNow().AddDays(7)
                };

                var inserted = await _refreshRepo.InsertAsync(newRefreshEntity, ct);
                if (!inserted)
                {
                    ClearTokensCookie();
                    return Result.Error<AuthTokenModel>(Result.DATA_NOT_CREATE.Code, "Không thể tạo phiên đăng nhập mới");
                }

                await _uow.CommitAsync(ct);

                var roles = await _userRoleRepo.Query(true)
                    .Where(ur => ur.UserId == tokenEntity.UserId)
                    .Join(_roleRepo.Query(true), ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .ToListAsync(ct);

                var newAccessToken = GenerateAccessToken(tokenEntity.UserId, user.UserName, user.SecurityStamp!, roles);
                SetTokensCookie(newAccessToken, newRefreshToken);

                return Result.Ok(new AuthTokenModel
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                });
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                var msg = "Đã có lỗi xảy ra khi làm mới phiên đăng nhập";
                _logger.Error(ex, msg);
                return Result.Exception<AuthTokenModel>(msg, ex);
            }
        }
        #endregion

        #region Cookie Management
        public void SetTokensCookie(string accessToken, string refreshToken)
        {
            var context = _httpContextAccessor.HttpContext!;

            context.Response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeHelper.UtcNow().AddMinutes(60)
            });

            context.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeHelper.UtcNow().AddDays(7)
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
