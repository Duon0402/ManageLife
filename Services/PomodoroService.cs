using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Models.Pomodoro;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class PomodoroService : ServiceBase<PomodoroService>, IPomodoroService
    {
        private readonly IPomodoroSessionRepository _repoSession;
        private readonly IPomodoroSettingRepository _repoSetting;

        public PomodoroService(
            IAppLogger<PomodoroService> logger,
            IUserContext userContext,
            IPomodoroSessionRepository repoSession,
            IPomodoroSettingRepository repoSetting)
            : base(logger, userContext)
        {
            _repoSession = repoSession;
            _repoSetting = repoSetting;
        }

        public async Task<Result> SaveSessionAsync(SavePomodoroSessionRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var validationError = Validate(request);
                if (validationError.IsNotEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, validationError);

                if (request.StartedAt == default)
                    return Result.Error(Result.DATA_INVALID.Code, "StartedAt không hợp lệ");

                var entity = new PomodoroSessionEntity
                {
                    UserId = userId,
                    Type = request.Type,
                    DurationMinutes = request.DurationMinutes,
                    StartedAt = request.StartedAt,
                    IsCompleted = request.IsCompleted
                };

                await _repoSession.InsertAsync(entity, ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Đã có lỗi xảy ra");
                return Result.Exception("Đã có lỗi xảy ra", ex);
            }
        }

        public async Task<Result> SaveSessionsAsync(List<SavePomodoroSessionRequest> requests, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                if (requests == null || requests.Count == 0)
                    return Result.Ok();

                foreach (var r in requests)
                {
                    var validationError = Validate(r);
                    if (validationError.IsNotEmpty())
                        return Result.Error(Result.DATA_INVALID.Code, validationError);

                    if (r.StartedAt == default)
                        return Result.Error(Result.DATA_INVALID.Code, "StartedAt không hợp lệ");
                }

                var entities = requests.Select(r => new PomodoroSessionEntity
                {
                    UserId = userId,
                    Type = r.Type,
                    DurationMinutes = r.DurationMinutes,
                    StartedAt = r.StartedAt,
                    IsCompleted = r.IsCompleted
                }).ToList();

                await _repoSession.BulkInsertAsync(entities, ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Đã có lỗi xảy ra");
                return Result.Exception("Đã có lỗi xảy ra", ex);
            }
        }

        public async Task<Result> SaveSettingAsync(SavePomodoroSettingRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var validationError = Validate(request);
                if (validationError.IsNotEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, validationError);

                var entity = await _repoSetting.FirstOrDefaultAsync(x => x.UserId == userId, ct);
                bool success;

                if (entity == null)
                {
                    entity = new PomodoroSettingEntity
                    {
                        UserId = userId,
                        YoutubeUrl = request.YoutubeUrl?.Trim(),
                        BackgroundFileId = request.BackgroundFileId,
                        FocusMinutes = request.FocusMinutes,
                        ShortBreakMinutes = request.ShortBreakMinutes,
                        LongBreakMinutes = request.LongBreakMinutes,
                        SessionLoops = request.SessionLoops
                    };
                    success = await _repoSetting.InsertAsync(entity, ct);
                }
                else
                {
                    entity.YoutubeUrl = request.YoutubeUrl?.Trim();
                    entity.BackgroundFileId = request.BackgroundFileId;
                    entity.FocusMinutes = request.FocusMinutes;
                    entity.ShortBreakMinutes = request.ShortBreakMinutes;
                    entity.LongBreakMinutes = request.LongBreakMinutes;
                    entity.SessionLoops = request.SessionLoops;
                    success = await _repoSetting.UpdateAsync(entity, ct);
                }

                if (!success)
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể lưu cấu hình");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Đã có lỗi xảy ra");
                return Result.Exception("Đã có lỗi xảy ra", ex);
            }
        }

        public async Task<Result<PomodoroSettingModel>> GetSettingsAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error<PomodoroSettingModel>(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var entity = await _repoSetting.FirstOrDefaultAsync(x => x.UserId == userId, ct);

                var model = entity == null
                    ? new PomodoroSettingModel
                    {
                        FocusMinutes = 25,
                        ShortBreakMinutes = 5,
                        LongBreakMinutes = 15
                    }
                    : new PomodoroSettingModel
                    {
                        Id = entity.Id,
                        UserId = entity.UserId,
                        YoutubeUrl = entity.YoutubeUrl,
                        BackgroundFileId = entity.BackgroundFileId,
                        FocusMinutes = entity.FocusMinutes,
                        ShortBreakMinutes = entity.ShortBreakMinutes,
                        LongBreakMinutes = entity.LongBreakMinutes,
                        SessionLoops = entity.SessionLoops
                    };

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Đã có lỗi xảy ra");
                return Result.Exception<PomodoroSettingModel>("Đã có lỗi xảy ra", ex);
            }
        }

        public async Task<Result<PomodoroHistoryModel>> GetHistoryAsync(int days = 7, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error<PomodoroHistoryModel>(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var from = DateTime.UtcNow.AddDays(-days);
                var sessions = await _repoSession.Query(true)
                    .Where(x => x.UserId == userId && x.StartedAt >= from)
                    .OrderByDescending(x => x.StartedAt)
                    .ToListAsync(ct);

                var model = new PomodoroHistoryModel
                {
                    Sessions = sessions.Select(s => new PomodoroSessionModel
                    {
                        Id = s.Id,
                        StartedAt = s.StartedAt,
                        DurationMinutes = s.DurationMinutes,
                        Type = s.Type,
                        IsCompleted = s.IsCompleted
                    }).ToList(),
                    TotalFocusMinutes = sessions
                        .Where(s => s.Type == PomodoroSessionType.Focus && s.IsCompleted)
                        .Sum(s => s.DurationMinutes),
                    CompletedFocusSessions = sessions
                        .Count(s => s.Type == PomodoroSessionType.Focus && s.IsCompleted)
                };

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Đã có lỗi xảy ra");
                return Result.Exception<PomodoroHistoryModel>("Đã có lỗi xảy ra", ex);
            }
        }
    }
}
