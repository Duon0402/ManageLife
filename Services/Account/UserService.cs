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

        public UserService(AppDbContext context) : base(context)
        {
            _repo = new UserRepository(context);
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
                    return Result.Error(Result.DATA_EXIST.Code, msg);
                }

                if (model.Password != model.ConfirmPassword)
                {
                    msg = "Mật khẩu không khớp";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }
                // TODO: Thêm kiểm tra mật khẩu (IsPasswordValid), và thêm Role (mặc định là User)
                var entity = new UserEntity()
                {
                    Id = IdHeper.NewId(),
                    UserName = model.UserName,
                    HashPassword = PasswordHelper.HashPassword(model.Password),
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
                    msg = "Tên đăng hoặc mật khẩu không đúng";
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
    }
}
