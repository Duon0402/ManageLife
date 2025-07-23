using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Helpers;
using ManageLife.Models;
using ManageLife.Repositories;

namespace ManageLife.Services
{
    public class UserService : ServiceBase
    {
        private readonly UserRepository _repo;
        private readonly RoleRepository _roleRepo;

        public UserService(AppDbContext context) : base(context)
        {
            _repo = new UserRepository(context);
            _roleRepo = new RoleRepository(context);
        }

        public async Task<Result> RegisterAsync(RegisterAccountModel model)
        {
            string msg;
            try
            {
                if (model == null)
                {
                    msg = "Dữ liệu đầu vào không hợp lệ";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var existedEntity = await _repo.GetAsync(x => x.UserName == model.UserName);

                if (existedEntity != null)
                {
                    msg = "Tên đăng nhập đã tồn tại";
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                if (model.Password != model.ConfirmPassword)
                {
                    msg = "Mật khẩu không khớp";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var userRole = await _roleRepo.GetAsync(x => x.Name == "User" && x.IsDeleted == false);
                if(userRole == null)
                {
                    msg = "Không thể đăng ký tài khoản";
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                var (isValid, passwordMsg) = IsPasswordValid(model.Password);
                if (!isValid)
                {
                    msg = passwordMsg;
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = new UserEntity()
                {
                    Id = IdHeper.NewId(),
                    UserName = model.UserName,
                    HashPassword = PasswordHelper.HashPassword(model.Password),
                    RoleId = userRole.Id,
                };

                var b = await _repo.InsertAsync(entity);

                if (!b)
                {
                    msg = "Không thể đăng ký tài khoản";
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi đăng ký tài khoản";
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> LoginAsync(LoginAccountModel model)
        {
            string msg;
            try
            {
                if (model == null)
                {
                    msg = "Dữ liệu đầu vào không hợp lệ";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(x => x.UserName == model.UserName);

                if (entity == null)
                {
                    msg = "Tên đăng nhập hoặc mật khẩu không đúng";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                if (PasswordHelper.HashPassword(model.Password) != entity.HashPassword)
                {
                    msg = "Tên đăng hoặc mật khẩu không đúng";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi đăng nhập tài khoản";
                return Result.Exception(msg, ex);
            }

            //TODO: Thêm đăng xuất, đổi mật khẩu
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

            return (true, string.Empty);
        }
    }
}
