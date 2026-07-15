using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Entities;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class UserService : ServiceBase<UserService>, IUserService
    {
        private const int RefreshTokenExpiryDays = 7;

        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IUserRefreshTokenRepository _refreshRepo;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _uow;
        private readonly ISettingContext _settingContext;

        public UserService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IUserRoleRepository userRoleRepo,
            IUserRefreshTokenRepository refreshRepo,
            ITokenService tokenService,
            IUnitOfWork uow,
            ISettingContext settingContext,
            IUserContext userContext,
            IAppLogger<UserService> logger) : base(logger, userContext)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _refreshRepo = refreshRepo;
            _tokenService = tokenService;
            _uow = uow;
            _settingContext = settingContext;
        }

        public async Task<Result> RegisterAsync(RegisterAccountRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var existedUser = await _userRepo.FirstOrDefaultAsync(x => x.UserName == request.UserName, ct);
                if (existedUser != null)
                {
                    _logger.Debug("Tên đăng nhập đã tồn tại");
                    return Result.Error(Result.DATA_EXISTED.Code, "Tên đăng nhập đã tồn tại");
                }

                var roleEntity = await _roleRepo.FirstOrDefaultAsync(x => x.Name == "User" && x.IsDeleted == false, ct);
                if (roleEntity == null)
                {
                    _logger.Debug("Không thể đăng ký tài khoản: không tìm thấy role User");
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể đăng ký tài khoản");
                }

                await _uow.BeginTransactionAsync(ct);

                var userEntity = new UserEntity
                {
                    Id = IdHelper.NewId(),
                    UserName = request.UserName,
                    HashPassword = PasswordHelper.HashPassword(request.Password),
                    SecurityStamp = IdHelper.NewId(),
                    CreatedUser = SystemUsers.System
                };
                var userCreated = await _userRepo.InsertAsync(userEntity, ct);
                if (!userCreated)
                {
                    _logger.Debug("Không thể tạo user entity");
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể đăng ký tài khoản");
                }

                var userRoleEntity = new UserRoleEntity
                {
                    UserId = userEntity.Id,
                    RoleId = roleEntity.Id
                };
                var roleAssigned = await _userRoleRepo.InsertAsync(userRoleEntity, ct);
                if (!roleAssigned)
                {
                    _logger.Debug("Không thể gán role cho user");
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể đăng ký tài khoản");
                }

                var refreshToken = _tokenService.GenerateRefreshToken();
                var refreshEntity = new UserRefreshTokenEntity
                {
                    Id = IdHelper.NewId(),
                    UserId = userEntity.Id,
                    RefreshToken = refreshToken,
                    ExpiryTime = DateTimeHelper.UtcNow().AddDays(RefreshTokenExpiryDays)
                };

                var tokenSaved = await _refreshRepo.InsertAsync(refreshEntity, ct);
                if (!tokenSaved)
                {
                    _logger.Debug("Không thể tạo refresh token");
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể tạo phiên đăng nhập");
                }

                await _uow.CommitAsync(ct);

                var roles = new List<string> { roleEntity.Name };
                var accessToken = _tokenService.GenerateAccessToken(userEntity.Id, userEntity.UserName, userEntity.SecurityStamp!, roles);
                await _tokenService.SetTokensCookieAsync(accessToken, refreshToken);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                var msg = "Đã có lỗi xảy ra khi đăng ký tài khoản";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> LoginAsync(LoginAccountRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var userEntity = await _userRepo.FirstOrDefaultAsync(x => x.UserName == request.UserName && !x.IsDeleted, ct);
                if (userEntity == null)
                {
                    _logger.Debug("Tên đăng nhập hoặc mật khẩu không đúng");
                    return Result.Error(Result.DATA_INVALID.Code, "Tên đăng nhập hoặc mật khẩu không đúng");
                }

                if (!userEntity.IsActive)
                {
                    _logger.Debug("Tài khoản bị khóa");
                    return Result.Error(Result.DATA_INVALID.Code, "Tài khoản của bạn đã bị khóa");
                }

                if (userEntity.LockoutEnd.HasValue)
                {
                    if (userEntity.LockoutEnd.Value > DateTimeHelper.UtcNow())
                    {
                        _logger.Debug("Tài khoản đang bị khóa tạm thời do đăng nhập sai nhiều lần");
                        return Result.Error(Result.DATA_INVALID.Code, "Tài khoản tạm thời bị khoá do đăng nhập sai quá nhiều lần, vui lòng thử lại sau");
                    }

                    // Lockout đã hết hạn — cấp lại lượt thử mới, tránh khoá vô thời hạn chỉ vì gõ sai 1 lần sau đó
                    userEntity.AccessFailedCount = 0;
                    userEntity.LockoutEnd = null;
                }

                var passwordValid = PasswordHelper.VerifyPassword(request.Password, userEntity.HashPassword);

                if (!passwordValid)
                {
                    _logger.Debug("Mật khẩu không đúng");

                    var maxLoginAttempts = await _settingContext.GetIntAsync(SettingKeys.Security.MaxLoginAttempts, 5);
                    var lockoutMinutes = await _settingContext.GetIntAsync(SettingKeys.Security.LockoutMinutes, 15);

                    userEntity.AccessFailedCount++;
                    if (userEntity.AccessFailedCount >= maxLoginAttempts)
                        userEntity.LockoutEnd = DateTimeHelper.UtcNow().AddMinutes(lockoutMinutes);

                    await _userRepo.UpdateAsync(userEntity, ct);

                    return Result.Error(Result.DATA_INVALID.Code, "Tên đăng nhập hoặc mật khẩu không đúng");
                }

                await _uow.BeginTransactionAsync(ct);

                bool needsUpdate = false;

                if (PasswordHelper.IsLegacyHash(userEntity.HashPassword))
                {
                    userEntity.HashPassword = PasswordHelper.HashPassword(request.Password);
                    needsUpdate = true;
                }

                if (userEntity.SecurityStamp.IsEmpty())
                {
                    userEntity.SecurityStamp = IdHelper.NewId();
                    needsUpdate = true;
                }

                if (userEntity.AccessFailedCount != 0 || userEntity.LockoutEnd.HasValue)
                {
                    userEntity.AccessFailedCount = 0;
                    userEntity.LockoutEnd = null;
                    needsUpdate = true;
                }

                if (needsUpdate)
                    await _userRepo.UpdateAsync(userEntity, ct);

                var cleanupResult = await _tokenService.CleanupRefreshTokensAsync(userEntity.Id, _uow, ct);
                if (!cleanupResult.IsOk())
                {
                    _logger.Debug("Không thể dọn dẹp token cũ");
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể dọn dẹp token cũ");
                }

                var refreshToken = _tokenService.GenerateRefreshToken();
                var refreshEntity = new UserRefreshTokenEntity
                {
                    Id = IdHelper.NewId(),
                    UserId = userEntity.Id,
                    RefreshToken = refreshToken,
                    ExpiryTime = DateTimeHelper.UtcNow().AddDays(RefreshTokenExpiryDays),
                };

                var tokenSaved = await _refreshRepo.InsertAsync(refreshEntity, ct);
                if (!tokenSaved)
                {
                    _logger.Debug("Không thể tạo refresh token");
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể tạo phiên đăng nhập");
                }

                await _uow.CommitAsync(ct);

                var roles = await _userRoleRepo.Query(true)
                    .Where(ur => ur.UserId == userEntity.Id)
                    .Join(_roleRepo.Query(true),
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => r.Name)
                    .ToListAsync(ct);

                var accessToken = _tokenService.GenerateAccessToken(userEntity.Id, userEntity.UserName, userEntity.SecurityStamp!, roles);
                await _tokenService.SetTokensCookieAsync(accessToken, refreshToken);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                var msg = "Đã có lỗi xảy ra khi đăng nhập tài khoản";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> LogoutAsync(string? refreshToken, CancellationToken ct = default)
        {
            try
            {
                if (refreshToken == null)
                {
                    _logger.Debug("Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.");
                    return Result.Error(Result.DATA_INVALID.Code, "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.");
                }

                var tokenEntity = await _refreshRepo.FirstOrDefaultAsync(r => r.RefreshToken == refreshToken && !r.IsRevoked);

                if (tokenEntity == null)
                {
                    return Result.Ok();
                }

                tokenEntity.IsRevoked = true;
                var updated = await _refreshRepo.UpdateAsync(tokenEntity);
                if (!updated)
                {
                    _logger.Debug("Không thể đăng xuất");
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể đăng xuất");
                }

                _tokenService.ClearTokensCookie();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Đã có lỗi xảy ra khi đăng xuất");
                return Result.Exception("Đã có lỗi xảy ra khi đăng xuất", ex);
            }
        }

        public async Task<Result> ChangePasswordAsync(ChangePasswordRequest request, string? refreshToken, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                if (refreshToken == null)
                {
                    _logger.Debug("Refresh token null khi đổi mật khẩu");
                    return Result.Error(Result.DATA_INVALID.Code, "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.");
                }

                var userId = _userContext.GetUserId();
                var user = await _userRepo.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive && !x.IsDeleted, ct);
                if (user == null)
                {
                    _logger.Debug("Không tìm thấy user khi đổi mật khẩu");
                    return Result.Error(Result.DATA_INVALID.Code, TranslationKey.Common.Message.DataInvalid);
                }

                var tokenEntity = await _refreshRepo.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken && x.UserId == user.Id && x.IsRevoked == false, ct);
                if (tokenEntity == null)
                {
                    _logger.Debug("Token không hợp lệ khi đổi mật khẩu");
                    return Result.Error(Result.DATA_INVALID.Code, "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.");
                }

                var oldPasswordValid = PasswordHelper.VerifyPassword(request.OldPassword, user.HashPassword);

                if (!oldPasswordValid)
                {
                    _logger.Debug("Mật khẩu cũ không đúng");
                    return Result.Error(Result.DATA_INVALID.Code, "Mật khẩu cũ không đúng");
                }

                await _uow.BeginTransactionAsync(ct);

                user.HashPassword = PasswordHelper.HashPassword(request.NewPassword);
                user.SecurityStamp = IdHelper.NewId();
                var updated = await _userRepo.UpdateAsync(user, ct);
                if (!updated)
                {
                    _logger.Debug("Không thể cập nhật mật khẩu");
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, TranslationKey.Common.Message.UpdateError);
                }

                await _refreshRepo.Query()
                    .Where(x => x.UserId == user.Id)
                    .ExecuteDeleteAsync(ct);

                await _uow.CommitAsync(ct);

                await _tokenService.InvalidateSecurityStampCacheAsync(user.Id, ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, TranslationKey.Common.Message.SystemError);
                return Result.Exception(TranslationKey.Common.Message.SystemError, ex);
            }
        }

        #region Admin
        public async Task<Result<List<UserModel>>> GetListUsersAsync(CancellationToken ct = default)
        {
            try
            {
                var entities = await _userRepo.Query(true).Where(x => x.IsDeleted == false).ToListAsync(ct);
                var models = entities.MapToList<UserModel>();
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<List<UserModel>>(msg, ex);
            }
        }

        public async Task<Result<UserModel>> GetUserByIdAsync(GetUserByIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<UserModel>(Result.DATA_INVALID.Code, err);

                var entity = await _userRepo.FirstOrDefaultAsync(x => x.Id == request.UserId && x.IsDeleted == false, ct);
                if (entity == null)
                {
                    var msg = "User không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error<UserModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<UserModel>();
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<UserModel>(msg, ex);
            }
        }
        #endregion
    }
}