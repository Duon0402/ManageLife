using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class HabitService : ServiceBase<HabitService>, IHabitService
    {
        private readonly IHabitRepository _repo;

        public HabitService(IAppLogger<HabitService> logger, IUserContext userContext, IHabitRepository repo)
            : base(logger, userContext)
        {
            _repo = repo;
        }

        public async Task<Result<List<HabitModel>>> GetListAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                var models = await _repo.Query(true)
                    .Where(x => x.OwnerId == userId && !x.IsDeleted)
                    .OrderByDescending(x => x.CreatedTime)
                    .Select(x => new HabitModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        IsActive = x.IsActive,
                        CreatedTime = x.CreatedTime
                    })
                    .ToListAsync(ct);

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi lấy danh sách habits");
                return Result.Exception<List<HabitModel>>("Có lỗi xảy ra khi lấy danh sách habits", ex);
            }
        }

        public async Task<Result> CreateAsync(CreateHabitRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var entity = new HabitEntity
                {
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim(),
                    IsActive = true,
                    OwnerId = _userContext.GetUserId()!
                };

                var inserted = await _repo.InsertAsync(entity, ct);
                if (!inserted)
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể tạo habit");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi tạo habit");
                return Result.Exception("Có lỗi xảy ra khi tạo habit", ex);
            }
        }

        public async Task<Result> UpdateAsync(UpdateHabitRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                var entity = await _repo.FirstOrDefaultAsync(
                    x => x.Id == request.Id && x.OwnerId == userId && !x.IsDeleted, ct);

                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Habit không tồn tại");

                entity.Name = request.Name.Trim();
                entity.Description = request.Description?.Trim();
                entity.IsActive = request.IsActive;

                var updated = await _repo.UpdateAsync(entity, ct);
                if (!updated)
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể cập nhật habit");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi cập nhật habit");
                return Result.Exception("Có lỗi xảy ra khi cập nhật habit", ex);
            }
        }

        public async Task<Result> DeleteAsync(DeleteHabitRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                var entity = await _repo.FirstOrDefaultAsync(
                    x => x.Id == request.Id && x.OwnerId == userId && !x.IsDeleted, ct);

                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Habit không tồn tại");

                var deleted = await _repo.DeleteAsync(entity, ct);
                if (!deleted)
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xóa habit");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi xóa habit");
                return Result.Exception("Có lỗi xảy ra khi xóa habit", ex);
            }
        }
    }
}
