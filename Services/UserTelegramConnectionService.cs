using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class UserTelegramConnectionService : IUserTelegramConnectionService
    {
        private readonly IAppLogger<UserTelegramConnectionService> _logger;
        private readonly IUserTelegramConnectionRepository _repo;
        private readonly IUserRepository _repoUser;

        public UserTelegramConnectionService(
            IAppLogger<UserTelegramConnectionService> logger,
            IUserTelegramConnectionRepository repo,
            IUserRepository repoUser)
        {
            _logger = logger;
            _repo = repo;
            _repoUser = repoUser;
        }

        public async Task<Result> CreateUserTelegramConnectionAsync(CreateUserTelegramConnectionRequest request, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var user = await _repoUser.GetAsync(request.UserId);
                if (user == null)
                {
                    msg = "Người dùng không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var entity = request.MapTo<UserTelegramConnectionEntity>();
                var b = await _repo.InsertAsync(entity);
                if (!b)
                {
                    msg = "Không thể tạo kết nối Telegram cho người dùng";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Có lỗi xảy ra khi tạo kết nối Telegram cho người dùng";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteUserTelegramConnectionAsync(DeleteUserTelegramConnectionRequest request, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(request.Id);
                if (entity == null)
                {
                    msg = "Kết nối Telegram cho người dùng không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var b = await _repo.DeleteAsync(entity);
                if (!b)
                {
                    msg = "Không thể xóa kết nối Telegram cho người dùng";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Có lỗi xảy ra khi xóa kết nối Telegram cho người dùng";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result<List<UserTelegramConnectionModel>>> GetListUserTelegramConnectionsAsync(CancellationToken ct = default)
        {
            string msg;
            try
            {
                var entities = await _repo.FindAsync(x => x.IsDeleted == false);
                var models = entities.MapToList<UserTelegramConnectionModel>();
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = "Có lỗi xảy ra khi xóa kết nối Telegram cho người dùng";
                _logger.Error(ex, msg);
                return Result.Exception<List<UserTelegramConnectionModel>>(msg, ex);
            }
        }

        public async Task<Result<UserTelegramConnectionModel>> GetUserTelegramConnectionByChatIdAsync(GetUserTelegramConnectionByChatIdRequest request, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error<UserTelegramConnectionModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.FirstOrDefaultAsync(x => x.IsDeleted == false && x.ChatId == request.ChatId);
                if (entity == null)
                {
                    msg = "Kết nối Telegram cho người dùng không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error<UserTelegramConnectionModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<UserTelegramConnectionModel>();
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                msg = "Có lỗi xảy ra khi xóa kết nối Telegram cho người dùng";
                _logger.Error(ex, msg);
                return Result.Exception<UserTelegramConnectionModel>(msg, ex);
            }
        }

        public async Task<Result<UserTelegramConnectionModel>> GetUserTelegramConnectionByUserIdAsync(GetUserTelegramConnectionByUserIdRequest request, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error<UserTelegramConnectionModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.FirstOrDefaultAsync(x => x.IsDeleted == false && x.UserId == request.UserId);
                if (entity == null)
                {
                    msg = "Kết nối Telegram cho người dùng không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error<UserTelegramConnectionModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<UserTelegramConnectionModel>();
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                msg = "Có lỗi xảy ra khi xóa kết nối Telegram cho người dùng";
                _logger.Error(ex, msg);
                return Result.Exception<UserTelegramConnectionModel>(msg, ex);
            }
        }

        public async Task<Result> UpdateUserTelegramConnectionAsync(UpdateUserTelegramConnectionRequest request, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var user = await _repoUser.GetAsync(request.UserId);
                if (user == null)
                {
                    msg = "Người dùng không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var entity = request.MapTo<UserTelegramConnectionEntity>();
                var b = await _repo.UpdateAsync(entity);
                if (!b)
                {
                    msg = "Không thể cập nhật kết nối Telegram cho người dùng";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Có lỗi xảy ra khi cập nhật kết nối Telegram cho người dùng";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }
    }
}
