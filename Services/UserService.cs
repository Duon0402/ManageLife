using AutoMapper;
using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class UserService : ServiceBase, IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IUserRefreshTokenRepository _refreshRepo;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _uow;
        private readonly IAppLogger<UserService> _logger;

        public UserService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IUserRoleRepository userRoleRepo,
            IUserRefreshTokenRepository refreshRepo,
            ITokenService tokenService,
            IUnitOfWork uow,
            IMapper mapper,
            IUserContext userContext,
            IAppLogger<UserService> logger) : base(mapper, userContext)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _refreshRepo = refreshRepo;
            _tokenService = tokenService;
            _uow = uow;
            _logger = logger;
        }

        public async Task<Result> RegisterAsync(RegisterAccountRequest request, CancellationToken ct = default)
        {
            await _uow.BeginTransactionAsync(ct);
            string msg;
            bool b;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var existedUser = await _userRepo.FirstOrDefaultAsync(x => x.UserName == request.UserName, ct);
                if (existedUser != null)
                {
                    msg = "Tên đăng nhập đã tồn tại";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                var roleEntity = await _roleRepo.FirstOrDefaultAsync(x => x.Name == "User" && x.IsDeleted == false, ct);
                if (roleEntity == null)
                {
                    msg = "Không thể đăng ký tài khoản";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                var userEntity = new UserEntity
                {
                    Id = IdHelper.NewId(),
                    UserName = request.UserName,
                    HashPassword = PasswordHelper.HashPassword(request.Password),
                    SecurityStamp = IdHelper.NewId(),
                    CreatedUser = SystemUsers.System
                };
                b = await _userRepo.InsertAsync(userEntity, ct);
                if (!b)
                {
                    msg = "Không thể đăng ký tài khoản";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                var userRoleEntity = new UserRoleEntity
                {
                    UserId = userEntity.Id,
                    RoleId = roleEntity.Id
                };
                b = await _userRoleRepo.InsertAsync(userRoleEntity, ct);
                if (!b)
                {
                    msg = "Không thể đăng ký tài khoản";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                var refreshToken = _tokenService.GenerateRefreshToken();
                var refreshEntity = new UserRefreshTokenEntity
                {
                    Id = IdHelper.NewId(),
                    UserId = userEntity.Id,
                    RefreshToken = refreshToken,
                    ExpiryTime = DateTimeHelper.UtcNow().AddDays(7)
                };

                b = await _refreshRepo.InsertAsync(refreshEntity, ct);
                if (!b)
                {
                    msg = "Không thể tạo phiên đăng nhập";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                await _uow.CommitAsync(ct);

                var roles = new List<string> { roleEntity.Name };

                var accessToken = _tokenService.GenerateAccessToken(userEntity.Id, userEntity.UserName, userEntity.SecurityStamp!, roles);
                _tokenService.SetTokensCookie(accessToken, refreshToken);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi đăng ký tài khoản";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> LoginAsync(LoginAccountRequest request, CancellationToken ct = default)
        {
            await _uow.BeginTransactionAsync(ct);
            string msg;
            bool b;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var userEntity = await _userRepo.FirstOrDefaultAsync(x => x.UserName == request.UserName && !x.IsDeleted, ct);
                if (userEntity == null)
                {
                    msg = "Tên đăng nhập hoặc mật khẩu không đúng";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                if (!userEntity.IsActive)
                {
                    msg = "Tài khoản của bạn đã bị khóa";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var passwordValid = PasswordHelper.IsLegacyHash(userEntity.HashPassword)
                    ? PasswordHelper.VerifyLegacy(request.Password, userEntity.HashPassword)
                    : PasswordHelper.Verify(request.Password, userEntity.HashPassword);

                if (!passwordValid)
                {
                    msg = "Tên đăng nhập hoặc mật khẩu không đúng";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

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

                if (needsUpdate)
                    await _userRepo.UpdateAsync(userEntity, ct);

                var cleanupResult = await _tokenService.CleanupRefreshTokensAsync(userEntity.Id, _uow, ct);
                if (!cleanupResult.IsOk())
                {
                    msg = "Không thể dọn dẹp token cũ";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                }

                var refreshToken = _tokenService.GenerateRefreshToken();
                var refreshEntity = new UserRefreshTokenEntity
                {
                    Id = IdHelper.NewId(),
                    UserId = userEntity.Id,
                    RefreshToken = refreshToken,
                    ExpiryTime = DateTimeHelper.UtcNow().AddDays(7),
                };

                b = await _refreshRepo.InsertAsync(refreshEntity, ct);
                if (!b)
                {
                    msg = "Không thể tạo phiên đăng nhập";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                await _uow.CommitAsync(ct);

                var roles = await _userRoleRepo.Query(true)
                    .Where(ur => ur.UserId == userEntity.Id)
                    .Join(_roleRepo.Query(true),
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => r.Name)
                    .ToListAsync(ct);

                var accessToken = this._tokenService.GenerateAccessToken(userEntity.Id, userEntity.UserName, userEntity.SecurityStamp!, roles);

                _tokenService.SetTokensCookie(accessToken, refreshToken);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi đăng nhập tài khoản";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> LogoutAsync(string? refreshToken, CancellationToken ct = default)
        {
            string msg;
            bool b;
            try
            {
                if (refreshToken == null)
                {
                    msg = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var tokenEntity = await _refreshRepo.FirstOrDefaultAsync(r => r.RefreshToken == refreshToken && !r.IsRevoked);

                if (tokenEntity == null)
                {
                    return Result.Ok();
                }

                tokenEntity.IsRevoked = true;
                b = await _refreshRepo.UpdateAsync(tokenEntity);
                if (!b)
                {
                    msg = "Không thể đăng xuất";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                }

                _tokenService.ClearTokensCookie();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi đăng xuất";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> ChangePasswordAsync(ChangePasswordRequest request, string? refreshToken, CancellationToken ct = default)
        {
            string msg;
            bool b;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                if (refreshToken == null)
                {
                    msg = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var userId = _userContext.GetUserId();
                var user = await _userRepo.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive && !x.IsDeleted);
                if (user == null)
                {
                    msg = TranslationKey.Common.Message.DataInvalid;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var tokenEntity = await _refreshRepo.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken && x.UserId == user.Id && x.IsRevoked == false);

                if (tokenEntity == null)
                {
                    msg = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var oldPasswordValid = PasswordHelper.IsLegacyHash(user.HashPassword)
                    ? PasswordHelper.VerifyLegacy(request.OldPassword, user.HashPassword)
                    : PasswordHelper.Verify(request.OldPassword, user.HashPassword);

                if (!oldPasswordValid)
                {
                    msg = "Mật khẩu cũ không đúng";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                await _uow.BeginTransactionAsync();

                user.HashPassword = PasswordHelper.HashPassword(request.NewPassword);
                user.SecurityStamp = IdHelper.NewId();
                b = await _userRepo.UpdateAsync(user);
                if (!b)
                {
                    msg = TranslationKey.Common.Message.UpdateError;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                }

                await _refreshRepo.Query()
                    .Where(x => x.UserId == user.Id)
                    .ExecuteDeleteAsync(ct);

                await _uow.CommitAsync();

                await _tokenService.InvalidateSecurityStampCacheAsync(user.Id, ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        #region Admin
        public async Task<Result<List<UserModel>>> GetListUsersAsync(CancellationToken ct = default)
        {
            try
            {
                var entities = await _userRepo.Query(true).Where(x => x.IsDeleted == false).ToListAsync();
                var models = entities.MapToList<UserModel>();
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = $"Đã có lỗi xảy ra: {ex.Message}";
                _logger.Error(ex, msg);
                return Result.Exception<List<UserModel>>(msg, ex);
            }
        }

        public async Task<Result<UserModel>> GetUserByIdAsync(GetUserByIdRequest request, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error<UserModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _userRepo.FirstOrDefaultAsync(x => x.Id == request.UserId && x.IsDeleted == false);
                if (entity == null)
                {
                    msg = "User không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error<UserModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<UserModel>();
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                msg = $"Đã có lỗi xảy ra: {ex.Message}";
                _logger.Error(ex, msg);
                return Result.Exception<UserModel>(msg, ex);
            }
        }
        #endregion
    }
}