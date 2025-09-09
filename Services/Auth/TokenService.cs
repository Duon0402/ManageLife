using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ManageLife.Services
{
    public class TokenService : ServiceBase, ITokenService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserRefreshTokenRepository _refreshRepo;
        private readonly UserRepository _userRepo;

        public TokenService(AppDbContext context, IConfiguration config, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _refreshRepo = new UserRefreshTokenRepository(context);
            _userRepo = new UserRepository(context);
        }

        #region Access Token
        public string GenerateAccessToken(string userId, string username, IEnumerable<string> roles)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new(JwtRegisteredClaimNames.UniqueName, username)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(2),
                                //expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateAccessToken(string? token)
        {
            if (token.IsEmpty())
            {
                return null;
            }

            var handler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)),
                ClockSkew = TimeSpan.Zero
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
        #endregion

        #region Refresh Token
        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        public async Task<Result> RefreshTokenAsync(string? refreshToken)
        {
            using var uow = await UnitOfWork.CreateAsync(_context);

            if (string.IsNullOrEmpty(refreshToken))
                return Result.Error(Result.DATA_INVALID.Code, "Refresh Token không hợp lệ");

            var tokenEntity = await _refreshRepo.Query()
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.RefreshToken == refreshToken &&
                                          r.ExpiryTime > DateTimeHelper.UtcNow() &&
                                          !r.IsRevoked);

            if (tokenEntity?.User == null || tokenEntity.User.IsDeleted || !tokenEntity.User.IsActive)
                return Result.Error(Result.DATA_INVALID.Code, "Phiên đăng nhập không hợp lệ hoặc đã hết hạn");

            // Revoke old token
            tokenEntity.IsRevoked = true;
            if (!await _refreshRepo.UpdateAsync(tokenEntity, uow))
            {
                await uow.RollbackAsync();
                return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể tạo phiên đăng nhập mới");
            }

            // Insert new refresh token
            var newRefreshToken = GenerateRefreshToken();
            var newRefreshEntity = new UserRefreshTokenEntity
            {
                Id = IdHeper.NewId(),
                UserId = tokenEntity.UserId,
                RefreshToken = newRefreshToken,
                ExpiryTime = DateTimeHelper.UtcNow().AddDays(7)
            };

            if (!await _refreshRepo.InsertAsync(newRefreshEntity, uow))
            {
                await uow.RollbackAsync();
                return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể tạo phiên đăng nhập mới");
            }

            await uow.CommitAsync();

            var roles = await _userRepo.Query()
                .Where(u => u.Id == tokenEntity.UserId)
                .SelectMany(u => u.UserRoles.Select(ur => ur.Role.Name))
                .ToListAsync();

            // Set cookies
            SetTokensCookie(tokenEntity.UserId, tokenEntity.User.UserName, roles, newRefreshToken);

            return Result.Ok();
        }
        #endregion

        #region Cookie Management
        public void SetTokensCookie(string userId, string username, IEnumerable<string> roles, string refreshToken)
        {
            var accessToken = GenerateAccessToken(userId, username, roles);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            var context = _httpContextAccessor.HttpContext!;
            context.Response.Cookies.Append("accessToken", accessToken, cookieOptions);
            context.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        public void ClearTokensCookie()
        {
            var context = _httpContextAccessor.HttpContext!;
            context.Response.Cookies.Delete("accessToken");
            context.Response.Cookies.Delete("refreshToken");
        }
        #endregion
    }
}
