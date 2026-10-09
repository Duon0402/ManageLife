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
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IUserRefreshTokenRepository _refreshRepo;
        private readonly IUserTelegramConnectionRepository _telegramRepo;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _uow;
        private readonly ISettingContext _settingContext;

        public UserService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IUserRoleRepository userRoleRepo,
            IUserRefreshTokenRepository refreshRepo,
            IUserTelegramConnectionRepository telegramRepo,
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
            _telegramRepo = telegramRepo;
            _tokenService = tokenService;
            _uow = uow;
            _settingContext = settingContext;
        }

        public async Task<Result<AuthTokenModel>> RegisterAsync(RegisterAccountRequest request, CancellationToken ct = default)
        {
            try
            {
                if (!await _settingContext.GetBoolAsync(SettingKeys.Feature.EnableRegistration, true))
                    return Result.Error<AuthTokenModel>("FEATURE_DISABLED", "Đăng ký tài khoản hiện đang tạm ngưng");

                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, err);

                var existedUser = await _userRepo.FirstOrDefaultAsync(x => x.UserName == request.UserName, ct);
                if (existedUser != null)
                {
                    _logger.Debug("Tên đăng nhập đã tồn tại");
                    return Result.Error<AuthTokenModel>(Result.DATA_EXISTED.Code, "Tên đăng nhập đã tồn tại");
                }

                var roleEntity = await _roleRepo.FirstOrDefaultAsync(x => x.Name == "User" && x.IsDeleted == false, ct);
                if (roleEntity == null)
                {
                    _logger.Debug("Không thể đăng ký tài khoản: không tìm thấy role User");
                    return Result.Error<AuthTokenModel>(Result.DATA_NOT_CREATE.Code, "Không thể đăng ký tài khoản");
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
                var roleAssigned = userCreated && await _userRoleRepo.InsertAsync(new UserRoleEntity
                {
                    UserId = userEntity.Id,
                    RoleId = roleEntity.Id
                }, ct);
                if (!roleAssigned)
                {
                    await _uow.RollbackAsync(ct);
                    _logger.Debug("Không thể tạo user hoặc gán role");
                    return Result.Error<AuthTokenModel>(Result.DATA_NOT_CREATE.Code, "Không thể đăng ký tài khoản");
                }

                var issued = await _tokenService.IssueTokensAsync(userEntity, ct);
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
                var msg = "Đã có lỗi xảy ra khi đăng ký tài khoản";
                _logger.Error(ex, msg);
                return Result.Exception<AuthTokenModel>(msg, ex);
            }
        }

        public async Task<Result<AuthTokenModel>> LoginAsync(LoginAccountRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, err);

                var userEntity = await _userRepo.FirstOrDefaultAsync(x => x.UserName == request.UserName && !x.IsDeleted, ct);
                if (userEntity == null)
                {
                    _logger.Debug("Tên đăng nhập hoặc mật khẩu không đúng");
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, "Tên đăng nhập hoặc mật khẩu không đúng");
                }

                if (!userEntity.IsActive)
                {
                    _logger.Debug("Tài khoản bị khóa");
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, "Tài khoản của bạn đã bị khóa");
                }

                if (userEntity.LockoutEnd.HasValue)
                {
                    if (userEntity.LockoutEnd.Value > DateTimeHelper.UtcNow())
                    {
                        _logger.Debug("Tài khoản đang bị khóa tạm thời do đăng nhập sai nhiều lần");
                        return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, "Tài khoản tạm thời bị khoá do đăng nhập sai quá nhiều lần, vui lòng thử lại sau");
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

                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, "Tên đăng nhập hoặc mật khẩu không đúng");
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

                var issued = await _tokenService.IssueTokensAsync(userEntity, ct);
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
                var msg = "Đã có lỗi xảy ra khi đăng nhập tài khoản";
                _logger.Error(ex, msg);
                return Result.Exception<AuthTokenModel>(msg, ex);
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

                // Tra cả token đã bị thay (vừa làm mới song song): đăng xuất theo cả phiên, không theo 1 token
                var tokenHash = _tokenService.HashRefreshToken(refreshToken);
                var tokenEntity = await _refreshRepo.FirstOrDefaultAsync(r => r.RefreshToken == tokenHash);

                if (tokenEntity == null || tokenEntity.SessionId.IsEmpty())
                {
                    return Result.Ok();
                }

                await _tokenService.RevokeSessionsAsync(tokenEntity.UserId, new[] { tokenEntity.SessionId }, ct);
                // Kể cả khi phiên đã thu hồi hết từ trước: vẫn chặn access token còn hạn của phiên này
                await _tokenService.MarkSessionsRevokedAsync(new[] { tokenEntity.SessionId });

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Đã có lỗi xảy ra khi đăng xuất");
                return Result.Exception("Đã có lỗi xảy ra khi đăng xuất", ex);
            }
        }

        public async Task<Result<AuthTokenModel>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                var user = await _userRepo.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive && !x.IsDeleted, ct);
                if (user == null)
                {
                    _logger.Debug("Không tìm thấy user khi đổi mật khẩu");
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, TranslationKey.Common.Message.DataInvalid);
                }

                if (!PasswordHelper.VerifyPassword(request.OldPassword, user.HashPassword))
                {
                    _logger.Debug("Mật khẩu cũ không đúng");
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, "Mật khẩu cũ không đúng");
                }

                await _uow.BeginTransactionAsync(ct);

                user.HashPassword = PasswordHelper.HashPassword(request.NewPassword);
                user.SecurityStamp = IdHelper.NewId();
                var updated = await _userRepo.UpdateAsync(user, ct);
                if (!updated)
                {
                    await _uow.RollbackAsync(ct);
                    _logger.Debug("Không thể cập nhật mật khẩu");
                    return Result.Error<AuthTokenModel>(Result.DATA_NOT_UPDATE.Code, TranslationKey.Common.Message.UpdateError);
                }

                // Đăng xuất mọi thiết bị khác, cấp phiên mới cho thiết bị đang đổi mật khẩu
                await _refreshRepo.Query()
                    .Where(x => x.UserId == user.Id)
                    .ExecuteDeleteAsync(ct);

                var issued = await _tokenService.IssueTokensAsync(user, ct);
                if (!issued.IsOk())
                {
                    await _uow.RollbackAsync(ct);
                    return issued;
                }

                await _uow.CommitAsync(ct);

                await _tokenService.InvalidateSecurityStampCacheAsync(user.Id, ct);

                return issued;
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, TranslationKey.Common.Message.SystemError);
                return Result.Exception<AuthTokenModel>(TranslationKey.Common.Message.SystemError, ex);
            }
        }

        public async Task<Result<AccountModel>> GetMyAccountAsync(CancellationToken ct = default)
        {
            try
            {
                var user = await GetCurrentUserAsync(ct);
                if (user == null)
                    return Result.Error<AccountModel>(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy tài khoản");

                return Result.Ok(await BuildAccountModelAsync(user, ct));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, TranslationKey.Common.Message.SystemError);
                return Result.Exception<AccountModel>(TranslationKey.Common.Message.SystemError, ex);
            }
        }

        public async Task<Result<AccountModel>> UpdateMyAccountAsync(UpdateAccountRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<AccountModel>(Result.DATA_INVALID.Code, err);

                var user = await GetCurrentUserAsync(ct);
                if (user == null)
                    return Result.Error<AccountModel>(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy tài khoản");

                user.FullName = request.FullName.IsEmpty() ? null : request.FullName!.Trim();
                user.Email = request.Email.IsEmpty() ? null : request.Email!.Trim();

                var updated = await _userRepo.UpdateAsync(user, ct);
                if (!updated)
                    return Result.Error<AccountModel>(Result.DATA_NOT_UPDATE.Code, TranslationKey.Common.Message.UpdateError);

                return Result.Ok(await BuildAccountModelAsync(user, ct));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, TranslationKey.Common.Message.SystemError);
                return Result.Exception<AccountModel>(TranslationKey.Common.Message.SystemError, ex);
            }
        }

        private async Task<UserEntity?> GetCurrentUserAsync(CancellationToken ct)
        {
            var userId = _userContext.GetUserId();
            if (userId.IsEmpty()) return null;
            return await _userRepo.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive && !x.IsDeleted, ct);
        }

        private async Task<AccountModel> BuildAccountModelAsync(UserEntity user, CancellationToken ct)
        {
            var roles = await _userRoleRepo.Query(true)
                .Where(ur => ur.UserId == user.Id)
                .Join(_roleRepo.Query(true), ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync(ct);

            var telegramLinked = await _telegramRepo.Query(true)
                .AnyAsync(x => x.UserId == user.Id && !x.IsDeleted, ct);

            return new AccountModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Roles = roles,
                TelegramLinked = telegramLinked,
                CreatedTime = DateTime.SpecifyKind(user.CreatedTime, DateTimeKind.Utc)
            };
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