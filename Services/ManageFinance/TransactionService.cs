using LinqKit;
using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Models;
using ManageLife.Repositories;

namespace ManageLife.Services
{
    public class TransactionService : ServiceBase
    {
        private readonly TransactionRepository _repo;

        public TransactionService(AppDbContext context) : base(context)
        {
            _repo = new TransactionRepository(context);
        }

        public async Task<Result<List<TransactionModel>>> GetListData(TransactionFilterModel? searchModel = null)
        {
            string msg;
            try
            {
                var predicate = PredicateBuilder.New<TransactionEntity>(x => x.IsDeleted == false);

                if (searchModel != null)
                {
                    if (searchModel.TransitionType.HasValue)
                    {
                        predicate.And(x => x.TransitionType == searchModel.TransitionType);
                    }

                    if (!string.IsNullOrWhiteSpace(searchModel.TransactionCategoryId))
                    {
                        predicate.And(x => x.TransactionCategoryId == searchModel.TransactionCategoryId);
                    }

                    if (searchModel.FromDate.HasValue)
                    {
                        predicate.And(x => x.TransactionDate >= searchModel.FromDate);
                    }

                    if (searchModel.ToDate.HasValue)
                    {
                        predicate.And(x => x.TransactionDate <= searchModel.ToDate);
                    }
                }

                var entities = await _repo.FindAsync(predicate);
                var models = new List<TransactionModel>();

                if (entities != null)
                {
                    models = entities.MapToList<TransactionModel>();
                }

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi lấy danh sách giao dịch";
                return Result.Exception<List<TransactionModel>>(msg, ex);
            }
        }

        public async Task<Result<TransactionModel>> GetDataById(string transactionId)
        {
            string msg;
            try
            {
                if (!string.IsNullOrWhiteSpace(transactionId))
                {
                    msg = "Dữ liệu đầu vào không hợp lệ";
                    return Result.Error<TransactionModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(x => x.Id == transactionId && x.IsDeleted == false);
                var model = new TransactionModel();

                if (entity != null)
                {
                    model = entity.MapTo<TransactionModel>();
                }

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi lấy thông tin giao dịch";
                return Result.Exception<TransactionModel>(msg, ex);
            }
        }

        public async Task<Result> InsertAsync(TransactionModel model)
        {
            string msg;
            bool b;
            try
            {
                if (model == null)
                {
                    msg = "Dữ liệu đâu vào không hợp lệ";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = model.MapTo<TransactionEntity>();
                entity.Id = IdHeper.NewId();
                entity.CreatedUser = "Admin";
                entity.CreatedTime = DateTime.Now;

                b = await _repo.InsertAsync(entity);

                if (!b)
                {
                    msg = "Không thể thêm mới giao dịch";
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi thêm mới giao dịch";
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> UpdateAsync(TransactionModel model)
        {
            string msg;
            bool b;
            try
            {
                if (model == null)
                {
                    msg = "Dữ liệu đâu vào không hợp lệ";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(x => x.Id == model.Id && x.IsDeleted == false);

                if (entity == null)
                {
                    msg = "Giao dịch đã bị xóa hoặc không tồn tại";
                    return Result.Error(Result.DATA_NOT_EXIST.Code, msg);
                }

                entity = model.MapTo<TransactionEntity>();
                entity.UpdatedUser = "Admin";
                entity.UpdatedTime = DateTime.Now;

                b = await _repo.UpdateAsync(entity);

                if (!b)
                {
                    msg = "Không thể chỉnh sửa giao dịch";
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi chỉnh sửa giao dịch";
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteAsync(string transactionId)
        {
            string msg;
            bool b;
            try
            {
                if (string.IsNullOrWhiteSpace(transactionId))
                {
                    msg = "Dữ liệu đầu vào không hợp lệ";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(x => x.Id == transactionId);

                if (entity == null)
                {
                    msg = "Giao dịch đã bị xóa hoặc không tồn tại";
                    return Result.Error(Result.DATA_NOT_EXIST.Code, msg);
                }

                entity.DeletedUser = "Admin";
                entity.DeletedTime = DateTime.Now;
                entity.IsDeleted = true;

                b = await _repo.UpdateAsync(entity);

                if (!b)
                {
                    msg = "Không thể xóa giao dịch";
                    return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã xảy ra lỗi khi xóa giao dịch";
                return Result.Exception(msg, ex);
            }
        }
    }
}
