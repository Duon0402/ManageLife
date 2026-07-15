using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class SettingService : ISettingService
    {
        private const string PasswordMask = "••••••••";

        private readonly ISettingRepository _repo;
        private readonly ISettingContext _settingContext;

        public SettingService(ISettingRepository repo, ISettingContext settingContext)
        {
            _repo = repo;
            _settingContext = settingContext;
        }

        public async Task<Result<List<SettingModel>>> GetListSettingsAsync(GetListSettingsRequest request, CancellationToken ct = default)
        {
            try
            {
                var query = _repo.Query(true);
                var entities = await query
                    .OrderBy(x => x.Group)
                    .ThenBy(x => x.Key)
                    .Select(x => new SettingModel
                    {
                        Id = x.Id,
                        Key = x.Key,
                        Value = x.Value,
                        Type = x.Type,
                        Group = x.Group,
                        Description = x.Description
                    })
                    .ToListAsync(ct);

                foreach (var s in entities)
                {
                    if (s.Type == SettingType.Password && s.Value.IsNotEmpty())
                        s.Value = PasswordMask;
                }

                return Result.Ok(entities);
            }
            catch (Exception ex)
            {
                return Result.Exception<List<SettingModel>>("Lỗi khi lấy danh sách cấu hình", ex);
            }
        }

        public async Task<Result<SettingModel>> GetSettingByIdAsync(GetSettingByIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var entity = await _repo.GetAsync(request.Id, ct);
                if (entity == null)
                    return Result.Error<SettingModel>(Result.DATA_NOT_EXISTED.Code, "Cấu hình không tồn tại");

                return Result.Ok(MapToModel(entity));
            }
            catch (Exception ex)
            {
                return Result.Exception<SettingModel>("Lỗi khi lấy cấu hình", ex);
            }
        }

        public async Task<Result<SettingModel>> GetSettingByKeyAsync(GetSettingByKeyRequest request, CancellationToken ct = default)
        {
            try
            {
                var entity = await _repo.FirstOrDefaultAsync(x => x.Key == request.Key, ct);
                if (entity == null)
                    return Result.Error<SettingModel>(Result.DATA_NOT_EXISTED.Code, "Cấu hình không tồn tại");

                return Result.Ok(MapToModel(entity));
            }
            catch (Exception ex)
            {
                return Result.Exception<SettingModel>("Lỗi khi lấy cấu hình theo key", ex);
            }
        }

        public async Task<Result> CreateSettingAsync(CreateSettingRequest request, CancellationToken ct = default)
        {
            try
            {
                var exists = await _repo.FirstOrDefaultAsync(x => x.Key == request.Key, ct);
                if (exists != null)
                    return Result.Error(Result.DATA_EXISTED.Code, $"Key '{request.Key}' đã tồn tại");

                var entity = new SettingEntity
                {
                    Key = request.Key.Trim(),
                    Value = request.Value ?? string.Empty,
                    Type = request.Type
                };

                await _repo.InsertAsync(entity, ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Exception("Lỗi khi tạo cấu hình", ex);
            }
        }

        public async Task<Result> UpdateSettingAsync(UpdateSettingRequest request, CancellationToken ct = default)
        {
            try
            {
                var entity = await _repo.GetAsync(request.Id, ct);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Cấu hình không tồn tại");

                // Giá trị vẫn là placeholder ẩn (admin không đổi) — bỏ qua để không ghi đè mật khẩu thật
                if (entity.Type == SettingType.Password && request.Value == PasswordMask)
                    return Result.Ok();

                entity.Value = request.Value ?? string.Empty;
                await _repo.UpdateAsync(entity, ct);

                await _settingContext.InvalidateCacheAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Exception("Lỗi khi cập nhật cấu hình", ex);
            }
        }

        public async Task<Result> DeleteSettingAsync(DeleteSettingRequest request, CancellationToken ct = default)
        {
            try
            {
                if (request.Id.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không có ID cấu hình");

                var entity = await _repo.GetAsync(request.Id!, ct);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Cấu hình không tồn tại");

                await _repo.DeleteAsync(entity, ct);
                await _settingContext.InvalidateCacheAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Exception("Lỗi khi xóa cấu hình", ex);
            }
        }

        public async Task RegisterSettingsAsync(List<SettingModel> settings, CancellationToken ct = default)
        {
            var registeredKeys = settings.Select(s => s.Key).ToHashSet();
            var allInDb = (await _repo.GetAllAsync(ct)).ToList();

            // Xóa settings không còn được đăng ký trong code
            var toDelete = allInDb.Where(e => !registeredKeys.Contains(e.Key)).ToList();
            if (toDelete.Count > 0)
                await _repo.BulkDeleteAsync(toDelete, ct);

            // Insert mới hoặc cập nhật metadata
            var dbByKey = allInDb.ToDictionary(e => e.Key);
            foreach (var s in settings)
            {
                if (!dbByKey.TryGetValue(s.Key, out var existing))
                {
                    var entity = new SettingEntity
                    {
                        Id = IdHelper.NewId(),
                        Key = s.Key,
                        Value = s.Value ?? string.Empty,
                        Type = s.Type,
                        Group = s.Group,
                        Description = s.Description
                    };
                    await _repo.InsertAsync(entity, ct);
                }
                else
                {
                    // Giữ nguyên Value — admin có thể đã chỉnh sửa
                    existing.Type = s.Type;
                    existing.Group = s.Group;
                    existing.Description = s.Description;
                    await _repo.UpdateAsync(existing, ct);
                }
            }

            await _settingContext.InvalidateCacheAsync();
        }

        private static SettingModel MapToModel(SettingEntity e) => new()
        {
            Id = e.Id,
            Key = e.Key,
            Value = e.Type == SettingType.Password && e.Value.IsNotEmpty() ? PasswordMask : e.Value,
            Type = e.Type,
            Group = e.Group,
            Description = e.Description
        };
    }
}
