using ManageLife.Core;
using ManageLife.Contexts;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class TelegramBotCommandService : ServiceBase<TelegramBotCommandService>, ITelegramBotCommandService
    {
        private readonly ITelegramBotCommandRepository _repo;

        public TelegramBotCommandService(
            ITelegramBotCommandRepository repo,
            IAppLogger<TelegramBotCommandService> logger,
            IUserContext userContext) : base(logger, userContext)
        {
            _repo = repo;
        }

        public async Task<Result<List<TelegramBotCommandModel>>> GetListAsync(CancellationToken ct = default)
        {
            try
            {
                var models = await _repo.Query(true)
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.Command)
                    .Select(x => new TelegramBotCommandModel
                    {
                        Id = x.Id,
                        Command = x.Command,
                        Description = x.Description,
                        SortOrder = x.SortOrder
                    })
                    .ToListAsync(ct);

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi lấy danh sách bot commands";
                _logger.Error(ex, msg);
                return Result.Exception<List<TelegramBotCommandModel>>(msg, ex);
            }
        }

        public async Task<Result> CreateAsync(CreateTelegramBotCommandRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var commandValue = request.Command.TrimStart('/').ToLower();
                var existed = await _repo.FirstOrDefaultAsync(x => x.IsDeleted == false && x.Command == commandValue, ct);
                if (existed != null)
                    return Result.Error(Result.DATA_EXISTED.Code, $"Command /{commandValue} đã tồn tại");

                var entity = new TelegramBotCommandEntity
                {
                    Id = IdHelper.NewId(),
                    Command = commandValue,
                    Description = request.Description.Trim(),
                    SortOrder = request.SortOrder
                };

                var inserted = await _repo.InsertAsync(entity, ct);
                if (!inserted)
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể tạo bot command");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi tạo bot command";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> UpdateAsync(UpdateTelegramBotCommandRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(request.Id, ct);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Bot command không tồn tại");

                var commandValue = request.Command.TrimStart('/').ToLower();
                var existed = await _repo.FirstOrDefaultAsync(x => x.IsDeleted == false && x.Command == commandValue && x.Id != request.Id, ct);
                if (existed != null)
                    return Result.Error(Result.DATA_EXISTED.Code, $"Command /{commandValue} đã tồn tại");

                entity.Command = commandValue;
                entity.Description = request.Description.Trim();
                entity.SortOrder = request.SortOrder;

                var updated = await _repo.UpdateAsync(entity, ct);
                if (!updated)
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể cập nhật bot command");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi cập nhật bot command";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteAsync(DeleteTelegramBotCommandRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(request.Id, ct);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Bot command không tồn tại");

                var deleted = await _repo.DeleteAsync(entity, ct);
                if (!deleted)
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xóa bot command");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi xóa bot command";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }
    }
}
