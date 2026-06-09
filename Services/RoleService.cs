using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Contexts;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class RoleService : ServiceBase<RoleService>, IRoleService
    {
        private readonly IRoleRepository _repoRole;
        private readonly IUserRoleRepository _userRoleRepo;

        public RoleService(IRoleRepository repoRole, IUserRoleRepository userRoleRepo, IAppLogger<RoleService> logger, IUserContext userContext) : base(logger, userContext)
        {
            _repoRole = repoRole;
            _userRoleRepo = userRoleRepo;
        }

        public async Task<Result> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var roleExisted = await _repoRole.FirstOrDefaultAsync(x => x.Name == request.Name.Trim() && x.IsDeleted == false, ct);
                if (roleExisted != null)
                {
                    var msg = $"Role [{request.Name}] đã tồn tại trong hệ thống";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                var entity = request.MapTo<RoleEntity>();

                var inserted = await _repoRole.InsertAsync(entity, ct);
                if (!inserted)
                {
                    var msg = $"Thêm mới Role [{request.Name}] không thành công";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = $"Đã có lỗi xảy ra: {ex.Message}";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> UpdateRoleAsync(UpdateRoleRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var entity = await _repoRole.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted == false, ct);
                if (entity == null)
                {
                    var msg = "Không tìm thấy Role";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var nameConflict = await _repoRole.FirstOrDefaultAsync(x => x.Name == request.Name.Trim() && x.Id != request.Id && x.IsDeleted == false, ct);
                if (nameConflict != null)
                {
                    var msg = $"Role [{request.Name}] đã tồn tại trong hệ thống";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                entity.Code = request.Code.Trim();
                entity.Name = request.Name.Trim();
                entity.Description = request.Description;

                var updated = await _repoRole.UpdateAsync(entity, ct);
                if (!updated)
                {
                    var msg = $"Cập nhật Role [{request.Name}] không thành công";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = $"Đã có lỗi xảy ra: {ex.Message}";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteRoleAsync(DeleteRoleRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var entity = await _repoRole.FirstOrDefaultAsync(x => x.Id == request.RoleId && x.IsDeleted == false, ct);
                if (entity == null)
                {
                    var msg = "Không tìm thấy Role";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var deleted = await _repoRole.DeleteAsync(entity, ct);
                if (!deleted)
                {
                    var msg = "Không thể xóa Role";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = $"Đã có lỗi xảy ra: {ex.Message}";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result<List<RoleModel>>> GetListRolesAsync(CancellationToken ct = default)
        {
            try
            {
                var entities = await _repoRole.FindAsync(x => x.IsDeleted == false, ct);
                var models = entities.MapToList<RoleModel>();
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = $"Đã có lỗi xảy ra: {ex.Message}";
                _logger.Error(ex, msg);
                return Result.Exception<List<RoleModel>>(msg, ex);
            }
        }

        public async Task<Result<List<RoleModel>>> GetListRolesByUserIdAsync(GetListRolesByUserIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<List<RoleModel>>(Result.DATA_INVALID.Code, err);

                var roles = await _userRoleRepo.Query(true)
                    .Where(ur => ur.UserId == request.UserId)
                    .Join(
                        _repoRole.Query(true),
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => r
                    )
                    .Where(r => !r.IsDeleted)
                    .ToListAsync(ct);

                var models = roles.MapToList<RoleModel>();
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = $"Đã có lỗi xảy ra: {ex.Message}";
                _logger.Error(ex, msg);
                return Result.Exception<List<RoleModel>>(msg, ex);
            }
        }

        public async Task<Result<RoleModel>> GetRoleByIdAsync(GetRoleByIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<RoleModel>(Result.DATA_INVALID.Code, err);

                var entity = await _repoRole.FirstOrDefaultAsync(x => x.IsDeleted == false && x.Id == request.RoleId, ct);

                if (entity == null)
                {
                    var msg = "Không tìm thấy Role";
                    _logger.Debug(msg);
                    return Result.Error<RoleModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<RoleModel>();

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                var msg = $"Đã có lỗi xảy ra: {ex.Message}";
                _logger.Error(ex, msg);
                return Result.Exception<RoleModel>(msg, ex);
            }
        }

    }
}
