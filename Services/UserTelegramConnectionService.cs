using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Contexts;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class UserTelegramConnectionService : ServiceBase<UserTelegramConnectionService>, IUserTelegramConnectionService
    {
        private readonly IUserTelegramConnectionRepository _repo;
        private readonly IUserRepository _repoUser;

        public UserTelegramConnectionService(
            IUserTelegramConnectionRepository repo,
            IUserRepository repoUser,
            IAppLogger<UserTelegramConnectionService> logger,
            IUserContext userContext) : base(logger, userContext)
        {
            _repo = repo;
            _repoUser = repoUser;
        }

        public async Task<Result<List<UserTelegramConnectionModel>>> GetListUserTelegramConnectionsAsync(CancellationToken ct = default)
        {
            try
            {
                var models = await _repo.Query(true)
                    .Where(x => !x.IsDeleted)
                    .Join(_repoUser.Query(true),
                        c => c.UserId,
                        u => u.Id,
                        (c, u) => new UserTelegramConnectionModel
                        {
                            Id = c.Id,
                            UserId = c.UserId,
                            UserName = u.UserName,
                            ChatId = c.ChatId
                        })
                    .ToListAsync(ct);

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi lấy danh sách kết nối Telegram";
                _logger.Error(ex, msg);
                return Result.Exception<List<UserTelegramConnectionModel>>(msg, ex);
            }
        }

        public async Task<Result<UserTelegramConnectionModel>> GetUserTelegramConnectionByUserIdAsync(GetUserTelegramConnectionByUserIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error<UserTelegramConnectionModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.FirstOrDefaultAsync(x => x.IsDeleted == false && x.UserId == request.UserId, ct);
                if (entity == null)
                {
                    var msg = "Kết nối Telegram cho người dùng không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error<UserTelegramConnectionModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<UserTelegramConnectionModel>();
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi lấy kết nối Telegram theo UserId";
                _logger.Error(ex, msg);
                return Result.Exception<UserTelegramConnectionModel>(msg, ex);
            }
        }

        public async Task<Result<UserTelegramConnectionModel>> GetUserTelegramConnectionByChatIdAsync(GetUserTelegramConnectionByChatIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error<UserTelegramConnectionModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.FirstOrDefaultAsync(x => x.IsDeleted == false && x.ChatId == request.ChatId, ct);
                if (entity == null)
                {
                    var msg = "Kết nối Telegram cho người dùng không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error<UserTelegramConnectionModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<UserTelegramConnectionModel>();
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi lấy kết nối Telegram theo ChatId";
                _logger.Error(ex, msg);
                return Result.Exception<UserTelegramConnectionModel>(msg, ex);
            }
        }

        public async Task<Result> CreateUserTelegramConnectionAsync(CreateUserTelegramConnectionRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var user = await _repoUser.GetAsync(request.UserId, ct);
                if (user == null)
                {
                    var msg = "Người dùng không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var entity = request.MapTo<UserTelegramConnectionEntity>();
                var inserted = await _repo.InsertAsync(entity, ct);
                if (!inserted)
                {
                    var msg = "Không thể tạo kết nối Telegram cho người dùng";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi tạo kết nối Telegram cho người dùng";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> UpdateUserTelegramConnectionAsync(UpdateUserTelegramConnectionRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(request.Id, ct);
                if (entity == null)
                {
                    var msg = "Kết nối Telegram không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var user = await _repoUser.GetAsync(request.UserId, ct);
                if (user == null)
                {
                    var msg = "Người dùng không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                entity.ChatId = request.ChatId;
                entity.UserId = request.UserId;

                var updated = await _repo.UpdateAsync(entity, ct);
                if (!updated)
                {
                    var msg = "Không thể cập nhật kết nối Telegram cho người dùng";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi cập nhật kết nối Telegram cho người dùng";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteUserTelegramConnectionAsync(DeleteUserTelegramConnectionRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(request.Id, ct);
                if (entity == null)
                {
                    var msg = "Kết nối Telegram cho người dùng không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var deleted = await _repo.DeleteAsync(entity, ct);
                if (!deleted)
                {
                    var msg = "Không thể xóa kết nối Telegram cho người dùng";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi xóa kết nối Telegram cho người dùng";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }
    }
}
