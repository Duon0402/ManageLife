using LinqKit;
using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Entities;
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
    public class TokenService : ITokenService
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
            IUserRefreshTokenRepository refreshRepo,
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IUserRoleRepository userRoleRepo,
            IOptions<JwtSettings> jwtOptions,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork uow,
            ICacheService cache)
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
                expires: DateTimeHelper.UtcNow().AddMinutes(30),
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
            string msg;
            bool b;
            try
            {
                await _uow.BeginTransactionAsync();

                if (refreshToken.IsEmpty())
                {
                    ClearTokensCookie();
                    msg = "Phiên đăng nhập không hợp lệ hoặc đã hết hạn";
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, msg);
                }

                var tokenEntity = await _refreshRepo.Query()
                    .FirstOrDefaultAsync(r => r.RefreshToken == refreshToken &&
                                              r.ExpiryTime > DateTimeHelper.UtcNow() &&
                                              r.IsRevoked == false);

                if (tokenEntity == null)
                {
                    ClearTokensCookie();
                    msg = "Phiên đăng nhập không hợp lệ hoặc đã hết hạn";
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, msg);
                }

                var user = await _userRepo.GetAsync(tokenEntity.UserId);

                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    ClearTokensCookie();
                    msg = "Phiên đăng nhập không hợp lệ hoặc đã hết hạn";
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, msg);
                }

                tokenEntity.IsRevoked = true;
                b = await _refreshRepo.UpdateAsync(tokenEntity);
                if (!b)
                {
                    ClearTokensCookie();
                    msg = "Không thể tạo phiên đăng nhập mới";
                    return Result.Error<AuthTokenModel>(Result.DATA_NOT_UPDATE.Code, msg);
                }

                var cleanupResult = await CleanupRefreshTokensAsync(tokenEntity.UserId);
                if (!cleanupResult.IsOk())
                {
                    msg = "Không thể dọn dẹp token cũ";
                    return Result.Error<AuthTokenModel>(Result.DATA_NOT_DELETE.Code, msg);
                }

                var newRefreshToken = GenerateRefreshToken();
                var newRefreshEntity = new UserRefreshTokenEntity
                {
                    Id = IdHelper.NewId(),
                    UserId = tokenEntity.UserId,
                    RefreshToken = newRefreshToken,
                    ExpiryTime = DateTimeHelper.UtcNow().AddDays(7)
                };

                b = await _refreshRepo.InsertAsync(newRefreshEntity);
                if (!b)
                {
                    ClearTokensCookie();
                    msg = "Không thể tạo phiên đăng nhập mới";
                    return Result.Error<AuthTokenModel>(Result.DATA_NOT_CREATE.Code, msg);
                }

                await _uow.CommitAsync();

                var roleIds = await _userRoleRepo.Query()
                    .Where(ur => ur.UserId == tokenEntity.UserId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                var roles = await _roleRepo.Query()
                    .Where(r => roleIds.Contains(r.Id))
                    .Select(r => r.Name)
                    .ToListAsync();

                var newAccessToken = GenerateAccessToken(tokenEntity.UserId, user.UserName, user.SecurityStamp!, roles);

                SetTokensCookie(newAccessToken, newRefreshToken);

                var authToken = new AuthTokenModel
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                };
                return Result.Ok(authToken);
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi làm mới phiên đăng nhập";
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
                Expires = DateTimeHelper.UtcNow().AddMinutes(30)
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
            string msg;
            bool b;
            try
            {
                var predicate = PredicateBuilder.New<UserRefreshTokenEntity>(x => x.ExpiryTime <= DateTimeHelper.UtcNow() || x.IsRevoked);

                if (userId.IsNotEmpty())
                {
                    predicate = predicate.And(x => x.UserId == userId);
                }

                var entities = await _refreshRepo.Query().Where(predicate).ToListAsync();

                if (entities.IsNotEmpty())
                {
                    b = await _refreshRepo.BulkDeleteAsync(entities);

                    if (!b)
                    {
                        msg = TranslationKey.Common.Message.DeleteError;
                        return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                    }
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception(msg, ex);
            }
        }
    }
}
