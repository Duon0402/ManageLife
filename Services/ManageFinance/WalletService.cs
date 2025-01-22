using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Models;
using ManageLife.Repositories;

namespace ManageLife.Services
{
    public class WalletService : ServiceBase
    {
        private readonly WalletRepository _repo;

        public WalletService(AppDbContext context) : base(context)
        {
            _repo = new WalletRepository(context);
        }

        public async Task<Result<List<WalletModel>>> GetListDataAsync()
        {
            string msg;
            try
            {
                var entities = await _repo.FindAsync(x => x.IsDeleted == false);
                var models = new List<WalletModel>();
                models = entities.MapToList<WalletModel>();

                if (entities.IsNotEmpty())
                {
                    models = entities.MapToList<WalletModel>();
                }

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi lấy danh sách ví";
                return Result.Exception<List<WalletModel>>(msg, ex);
            }
        }

        public async Task<Result<WalletModel>> GetDataByIdAsync(string walletId)
        {
            string msg;
            try
            {
                if (string.IsNullOrWhiteSpace(walletId))
                {
                    msg = "Dữ liệu đầu vào không hợp lệ";
                    return Result.Error<WalletModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(x => x.Id == walletId && x.IsDeleted == false);
                var model = new WalletModel();

                if (entity != null)
                {
                    model = entity.MapTo<WalletModel>();
                }

                return Result.Ok<WalletModel>(model);
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi lấy thông tin ví";
                return Result.Exception<WalletModel>(msg, ex);
            }
        }

        public async Task<Result> InsertAsync(WalletModel model)
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

                var entity = model.MapTo<WalletEntity>();

                entity.Id = IdHeper.NewId();
                entity.CreatedUser = "Admin";
                entity.CreatedTime = DateTime.Now;

                b = await _repo.InsertAsync(entity);

                if (!b)
                {
                    msg = "Không thể thêm mới ví";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi thêm mới ví";
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> UpdateAsync(WalletModel model)
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

                var entity = await _repo.GetAsync(model.Id);

                if (entity == null)
                {
                    msg = "Ví đã bị xóa hoặc không tồn tại";
                    return Result.Error(Result.DATA_NOT_EXIST.Code, msg);
                }

                entity = model.MapTo<WalletEntity>();

                entity.UpdatedUser = "Admin";
                entity.UpdatedTime = DateTime.Now;

                b = await _repo.UpdateAsync(entity);

                if (!b)
                {
                    msg = "Không thể chỉnh sửa ví";
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi chỉnh sửa ví";
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteAsync(string walletId)
        {
            string msg;
            bool b;
            try
            {
                if (string.IsNullOrWhiteSpace(walletId))
                {
                    msg = "Dữ liệu đầu vào không hợp lệ";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(walletId);

                if (entity == null)
                {
                    msg = "Ví đã bị xóa hoặc không tồn tại";
                    return Result.Error(Result.DATA_NOT_EXIST.Code, msg);
                }

                entity.DeletedTime = DateTime.Now;
                entity.DeletedUser = "admin";
                entity.IsDeleted = true;

                b = await _repo.UpdateAsync(entity);

                if (!b)
                {
                    msg = "Không thể xóa ví";
                    return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi xóa ví";
                return Result.Exception(msg, ex);
            }
        }
    }
}
