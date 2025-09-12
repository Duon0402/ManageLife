using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ManageLife.Services
{
    public class UserService : ServiceBase, IUserService
    {
        private readonly UserRepository _userRepo;
        private readonly RoleRepository _roleRepo;
        private readonly UserRoleRepository _userRoleRepo;
        private readonly UserRefreshTokenRepository _refreshRepo;
        private readonly ITokenService _tokenService;

        public UserService(AppDbContext context, IConfiguration config, ITokenService tokenService) : base(context)
        {
            _userRepo = new UserRepository(_context);
            _roleRepo = new RoleRepository(_context);
            _userRoleRepo = new UserRoleRepository(_context);
            _refreshRepo = new UserRefreshTokenRepository(_context);
            _tokenService = tokenService;
        }

        public async Task<Result> RegisterAsync(RegisterAccountModel model)
        {
            using var uow = await UnitOfWork.CreateAsync(_context);
            string msg;
            bool b;
            try
            {
                if (model == null)
                {
                    msg = "Dữ liệu đầu vào không hợp lệ";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                if (model.Password != model.ConfirmPassword)
                {
                    msg = "Mật khẩu xác nhận không khớp";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var (isValid, passwordMsg) = IsPasswordValid(model.Password);
                if (!isValid)
                {
                    msg = passwordMsg;
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var existedUser = await _userRepo.GetAsync(x => x.UserName == model.UserName);
                if (existedUser != null)
                {
                    msg = "Tên đăng nhập đã tồn tại";
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                var roleEntity = await _roleRepo.GetAsync(x => x.Name == "User" && x.IsDeleted == false);
                if (roleEntity == null)
                {
                    msg = "Không thể đăng ký tài khoản";
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                var userEntity = new UserEntity
                {
                    Id = IdHeper.NewId(),
                    UserName = model.UserName,
                    HashPassword = PasswordHelper.HashPassword(model.Password),
                    CreatedUser = SystemUsers.System
                };
                b = await _userRepo.InsertAsync(userEntity, uow);
                if (!b)
                {
                    await uow.RollbackAsync();
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
                    await uow.RollbackAsync();
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
                    await uow.RollbackAsync();
                    msg = "Không thể tạo phiên đăng nhập";
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                await uow.CommitAsync();

                var roles = new List<string> { roleEntity.Name };

                var accessToken = _tokenService.GenerateAccessToken(userEntity.Id, userEntity.UserName, roles);
                _tokenService.SetTokensCookie(accessToken, refreshToken);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await uow.RollbackAsync();
                msg = "Đã có lỗi xảy ra khi đăng ký tài khoản";
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> LoginAsync(LoginAccountModel model)
        {
            string msg;
            bool b;
            try
            {
                if (model == null)
                {
                    msg = "Dữ liệu đầu vào không hợp lệ";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var userEntity = await _userRepo.GetAsync(x => x.UserName == model.UserName && !x.IsDeleted);
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

                if (PasswordHelper.HashPassword(model.Password) != userEntity.HashPassword)
                {
                    msg = "Tên đăng nhập hoặc mật khẩu không đúng";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var refreshToken = _tokenService.GenerateRefreshToken();

                var refreshEntity = new UserRefreshTokenEntity
                {
                    Id = IdHeper.NewId(),
                    UserId = userEntity.Id,
                    RefreshToken = refreshToken,
                    ExpiryTime = DateTimeHelper.UtcNow().AddDays(7),
                };

                b = await _refreshRepo.InsertAsync(refreshEntity);

                if (!b)
                {
                    msg = "Không thể tạo phiên đăng nhập";
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                var roles = await _userRepo.Query()
                    .Where(u => u.Id == userEntity.Id)
                    .SelectMany(u => u.UserRoles.Select(ur => ur.Role.Name))
                    .ToListAsync();

                var accessToken = this._tokenService.GenerateAccessToken(userEntity.Id, userEntity.UserName, roles);

                _tokenService.SetTokensCookie(accessToken, refreshToken);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi đăng nhập tài khoản";
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> LogoutAsync(string refreshToken)
        {
            string msg;
            bool b;
            try
            {
                var tokenEntity = await _refreshRepo.GetAsync(r => r.RefreshToken == refreshToken && !r.IsRevoked);

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

        private (bool isSuccess, string msg) IsPasswordValid(string password)
        {
            if (string.IsNullOrEmpty(password))
                return (false, "Mật khẩu không được để trống");

            if (password.Length < 8)
                return (false, "Mật khẩu phải có ít nhất 8 ký tự");

            if (!password.Any(char.IsDigit))
                return (false, "Mật khẩu phải có ít nhất 1 chữ số");

            if (!password.Any(char.IsUpper))
                return (false, "Mật khẩu phải có ít nhất 1 chữ hoa");

            if (!password.Any(char.IsLower))
                return (false, "Mật khẩu phải có ít nhất 1 chữ thường");

            if (!password.Any(ch => "!@#$%^&*()-_=+[]{};:'\",.<>?/\\|`~".Contains(ch)))
                return (false, "Mật khẩu phải có ít nhất 1 ký tự đặc biệt");

            return (true, string.Empty);
        }


        //TODO: Kiểm tra token reuse (nếu refreshToken đã bị thu hồi, log lại hoặc khóa account)

        //TODO: Dọn dẹp token cũ (hết hạn hoặc bị thu hồi) trước khi tạo token mới
    }
}