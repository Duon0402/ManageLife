using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Models;
using ManageLife.Repositories;

namespace ManageLife.Services
{
    public class WalletService 
    {
        private readonly WalletRepository _repo;

        public WalletService(AppDbContext context) 
        {
            _repo = new WalletRepository(context);
        }

        public async Task<Result<List<WalletModel>>> GetData()
        {
            var entities = await _repo.FindAsync(x => x.IsDeleted == false);
            var models = new List<WalletModel>();
            return Result.Ok(models);
        }

        public async Task<Result> InsertAsync(WalletModel model)
        {
            string msg;
            bool b;
            try
            {
                if(model == null)
                {
                    msg = "Dữ liệu đầu vào không hợp lệ.";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = model.MapTo<WalletEntity>();

                b = await _repo.InsertAsync(entity);

                if(!b)
                {
                    msg = "Không thể thêm mới ví.";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi thêm mới ví.";
                return Result.Exception(msg, ex);
            }
        }
    }
}
