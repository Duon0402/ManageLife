using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class RoleService : ServiceBase, IRoleService
    {
        private readonly RoleRepository _repoRole;
        private readonly UserRoleRepository _userRoleRepo;

        public RoleService(AppDbContext context) : base(context)
        {
            _repoRole = new RoleRepository(context);
            _userRoleRepo = new UserRoleRepository(context);
        }

        public async Task<Result> CreateRoleAsync(CreateRoleRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var roleExisted = await _repoRole.GetAsync(x => x.Name == request.Name.Trim() && x.IsDeleted == false);
                if (roleExisted != null)
                {
                    msg = $"Role [{request.Name}] đã tồn tại trong hệ thống";
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                var entity = request.MapTo<RoleEntity>();

                var b = await _repoRole.InsertAsync(entity);
                if (!b)
                {
                    msg = $"Thêm mới Role [{request.Name}] không thành công";
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = $"Đã có lỗi xảy ra: {ex.Message}";
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result<List<RoleModel>>> GetListRolesAsync()
        {
            string msg;
            try
            {
                var entities = await _repoRole.FindAsync(x => x.IsDeleted == false);
                var models = entities.MapToList<RoleModel>();
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = $"Đã có lỗi xảy ra: {ex.Message}";
                return Result.Exception<List<RoleModel>>(msg, ex);
            }
        }

        public async Task<Result<List<RoleModel>>> GetListRolesByUserIdAsync(GetListRolesByUserIdRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error<List<RoleModel>>(Result.DATA_INVALID.Code, msg);
                }

                var roles = await _userRoleRepo.Query(true)
                    .Where(ur => ur.UserId == request.UserId)
                    .Join(
                        _repoRole.Query(true),
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => r
                    )
                    .Where(r => !r.IsDeleted)
                    .ToListAsync();

                var models = roles.MapToList<RoleModel>();
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = $"Đã có lỗi xảy ra: {ex.Message}";
                return Result.Exception<List<RoleModel>>(msg, ex);
            }
        }

        public async Task<Result<RoleModel>> GetRoleByIdAsync(GetRoleByIdRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error<RoleModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repoRole.GetAsync(x => x.IsDeleted == false && x.Id == request.RoleId);

                if (entity == null)
                {
                    msg = "Không tìm thấy Role";
                    return Result.Error<RoleModel>(Result.DATA_EXISTED.Code, msg);
                }

                var model = entity.MapTo<RoleModel>();

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                msg = $"Đã có lỗi xảy ra: {ex.Message}";
                return Result.Exception<RoleModel>(msg, ex);
            }
        }

    }
}
