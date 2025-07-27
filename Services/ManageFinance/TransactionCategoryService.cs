using LinqKit;
using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Models;
using ManageLife.Repositories;

namespace ManageLife.Services
{
	public class TransactionCategoryService : ServiceBase
	{
		private readonly TransactionCategoryRepository _repo;

		public TransactionCategoryService(AppDbContext context) : base(context)
		{
			_repo = new TransactionCategoryRepository(context);
		}

		public async Task<Result<List<TransactionCategoryModel>>> GetListData(TransactionCategoryFilterModel filterModel)
		{
			string msg;
			try
			{
				var predicate = PredicateBuilder.New<TransactionCategoryEntity>(x => x.IsDeleted == false);
				if (filterModel != null)
				{
					if (!string.IsNullOrEmpty(filterModel.Keyword))
					{
						predicate.And(x => x.Name.Contains(filterModel.Keyword));
					}

					if (filterModel.TransactionType.HasValue)
					{
						predicate.And(x => x.TransactionType == filterModel.TransactionType);
					}
				}

				var entities = await _repo.FindAsync(predicate);
				var models = new List<TransactionCategoryModel>();

				if (entities != null)
				{
					models = entities.MapToList<TransactionCategoryModel>();
				}

				return Result.Ok(models);
			}
			catch (Exception ex)
			{
				msg = "Đã có lỗi xảy ra khi lấy danh sách danh mục giao dịch";
				return Result.Exception<List<TransactionCategoryModel>>(msg, ex);
			}
		}
		public async Task<Result<TransactionCategoryModel>> GetDataById(string categoryId)
		{
			string msg;
			try
			{
				if (string.IsNullOrWhiteSpace(categoryId))
				{
					msg = "Dữ liệu đầu vào không hợp lệ";
					return Result.Error<TransactionCategoryModel>(Result.DATA_INVALID.Code, msg);
				}

				var entity = await _repo.GetAsync(x => x.Id == categoryId && x.IsDeleted == false);
				var model = new TransactionCategoryModel();

				if (entity != null)
				{
					model = entity.MapTo<TransactionCategoryModel>();
				}

				return Result.Ok(model);
			}
			catch (Exception ex)
			{
				msg = "Có lỗi xảy ra khi lấy về thông tin danh mục giao dịch";
				return Result.Exception<TransactionCategoryModel>(msg, ex);
			}
		}

		public async Task<Result> InsertAsync(TransactionCategoryModel model)
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

				var entity = model.MapTo<TransactionCategoryEntity>();
				entity.Id = IdHeper.NewId();
				entity.CreatedUser = "Admin";
				entity.CreatedTime = DateTime.Now;

				b = await _repo.InsertAsync(entity);

				if (!b)
				{
					msg = "Không thể thêm mới danh mục giao dịch";
					return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
				}

				return Result.Ok();
			}
			catch (Exception ex)
			{
				msg = "Đã có lỗi xảy ra khi thêm mới danh mục giao dịch";
				return Result.Exception(msg, ex);
			}
		}
	}
}
