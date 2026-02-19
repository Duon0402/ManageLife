using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ManageLife.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IUserRefreshTokenRepository _refreshRepo;
        private readonly ITokenService _tokenService;
        private readonly AppDbContext _context;

        public UserService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IUserRoleRepository userRoleRepo,
            IUserRefreshTokenRepository refreshRepo,
            ITokenService tokenService,
            AppDbContext context)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _refreshRepo = refreshRepo;
            _tokenService = tokenService;
            _context = context;
        }

        public async Task<Result> RegisterAsync(RegisterAccountRequest request)
        {
            using var uow = new UnitOfWork(_context);
            string msg;
            bool b;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var existedUser = await _userRepo.FirstOrDefaultAsync(x => x.UserName == request.UserName);
                if (existedUser != null)
                {
                    msg = "Tên đăng nhập đã tồn tại";
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                var roleEntity = await _roleRepo.FirstOrDefaultAsync(x => x.Name == "User" && x.IsDeleted == false);
                if (roleEntity == null)
                {
                    msg = "Không thể đăng ký tài khoản";
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                var userEntity = new UserEntity
                {
                    Id = IdHeper.NewId(),
                    UserName = request.UserName,
                    HashPassword = PasswordHelper.HashPassword(request.Password),
                    CreatedUser = SystemUsers.System
                };
                b = await _userRepo.InsertAsync(userEntity, uow);
                if (!b)
                {
                    msg = "Không thể đăng ký tài khoản";
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                var userRoleEntity = new UserRoleEntity
                {
                    UserId = userEntity.Id,
                    RoleId = roleEntity.Id
                };
                b = await _userRoleRepo.InsertAsync(userRoleEntity, uow);
                if (!b)
                {
                    msg = "Không thể đăng ký tài khoản";
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                var refreshToken = _tokenService.GenerateRefreshToken();
                var refreshEntity = new UserRefreshTokenEntity
                {
                    Id = IdHeper.NewId(),
                    UserId = userEntity.Id,
                    RefreshToken = refreshToken,
                    ExpiryTime = DateTimeHelper.UtcNow().AddDays(7)
                };

                b = await _refreshRepo.InsertAsync(refreshEntity, uow);
                if (!b)
                {
                    msg = "Không thể tạo phiên đăng nhập";
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                await uow.CommitAsync();

                var roles = new List<string> { roleEntity.Name };

                var accessToken = _tokenService.GenerateAccessToken(userEntity.Id, userEntity.UserName, IdHeper.NewId(), roles);
                _tokenService.SetTokensCookie(accessToken, refreshToken);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi đăng ký tài khoản";
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> LoginAsync(LoginAccountRequest request)
        {
            using var uow = new UnitOfWork(_context);
            string msg;
            bool b;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var userEntity = await _userRepo.FirstOrDefaultAsync(x => x.UserName == request.UserName && !x.IsDeleted);
                if (userEntity == null)
                {
                    msg = "Tên đăng nhập hoặc mật khẩu không đúng";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                if (!userEntity.IsActive)
                {
                    msg = "Tài khoản của bạn đã bị khóa";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                if (PasswordHelper.HashPassword(request.Password) != userEntity.HashPassword)
                {
                    msg = "Tên đăng nhập hoặc mật khẩu không đúng";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var cleanupResult = await _tokenService.CleanupRefreshTokensAsync(userEntity.Id, uow);
                if (!cleanupResult.IsOk())
                {
                    msg = "Không thể dọn dẹp token cũ";
                    return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                }

                var refreshToken = _tokenService.GenerateRefreshToken();
                var refreshEntity = new UserRefreshTokenEntity
                {
                    Id = IdHeper.NewId(),
                    UserId = userEntity.Id,
                    RefreshToken = refreshToken,
                    ExpiryTime = DateTimeHelper.UtcNow().AddDays(7),
                };

                b = await _refreshRepo.InsertAsync(refreshEntity, uow);
                if (!b)
                {
                    msg = "Không thể tạo phiên đăng nhập";
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                await uow.CommitAsync();

                var roles = await _userRepo.Query()
                    .Where(u => u.Id == userEntity.Id)
                    .SelectMany(u => u.UserRoles.Select(ur => ur.Role.Name))
                    .ToListAsync();

                var accessToken = this._tokenService.GenerateAccessToken(userEntity.Id, userEntity.UserName, IdHeper.NewId(), roles);

                _tokenService.SetTokensCookie(accessToken, refreshToken);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi đăng nhập tài khoản";
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> LogoutAsync(string? refreshToken)
        {
            string msg;
            bool b;
            try
            {
                if (refreshToken == null)
                {
                    msg = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
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
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                }

                _tokenService.ClearTokensCookie();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi đăng xuất";
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> ChangePasswordAsync(ChangePasswordRequest request, string? refreshToken)
        {
            string msg;
            bool b;
            try
            {
                var validate = request.Validate();

                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                if (refreshToken == null)
                {
                    msg = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var userId = UserContext.User?.GetUserId();
                var user = await _userRepo.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive == true && x.IsDeleted == true);
                if (user == null)
                {
                    msg = TranslationKey.Common.Message.DataInvalid;
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var tokenEntity = await _refreshRepo.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken && x.UserId == user.Id && x.IsRevoked == false);

                if (tokenEntity == null)
                {
                    msg = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                if (user.HashPassword != PasswordHelper.HashPassword(request.OldPassword))
                {
                    msg = "Mật khẩu cũ không đúng";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                using var uow = new UnitOfWork(_context);
                var newHashedPassword = PasswordHelper.HashPassword(request.NewPassword);
                user.HashPassword = newHashedPassword;
                b = await _userRepo.UpdateAsync(user, uow);
                if (!b)
                {
                    msg = TranslationKey.Common.Message.UpdateError;
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                }

                tokenEntity.IsRevoked = true;
                b = await _refreshRepo.UpdateAsync(tokenEntity, uow);
                if (!b)
                {
                    msg = "Không thể cập nhật token";
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                }
                await uow.CommitAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception(msg, ex);
            }
        }

        #region Admin
        public async Task<Result<List<UserModel>>> GetListUsersAsync()
        {
            try
            {
                var entities = await _userRepo.Query().Where(x => x.IsDeleted == false).ToListAsync();
                var models = entities.MapToList<UserModel>();
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                string msg = $"Đã có lỗi xảy ra: {ex.Message}";
                return Result.Exception<List<UserModel>>(msg, ex);
            }
        }

        public async Task<Result<UserModel>> GetUserByIdAsync(GetUserByIdRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error<UserModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _userRepo.FirstOrDefaultAsync(x => x.Id == request.UserId && x.IsDeleted == false);
                if (entity == null)
                {
                    msg = "User không tồn tại";
                    return Result.Error<UserModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<UserModel>();
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                msg = $"Đã có lỗi xảy ra: {ex.Message}";
                return Result.Exception<UserModel>(msg, ex);
            }
        }
        #endregion
    }
}