using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Models.Pomodoro;

namespace ManageLife.Services
{
    public class PomodoroService : ServiceBase<PomodoroService>, IPomodoroService
    {
        public PomodoroService(IAppLogger<PomodoroService> logger, IUserContext userContext) : base(logger, userContext)
        {
        }

        public async Task<Result> SaveSessionAsync(SavePomodoroSessionRequest request)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty())
                {
                    _logger.Debug(err);
                    return Result.Error(Result.DATA_INVALID.Code, err);
                }

                var entity = request.MapTo<PomodoroSessionEntity>();
    

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Đã có lỗi xảy ra: {ex.Message}");
                return Result.Exception("Đã có lỗi xảy ra", ex);
            }
        }

        public async Task<Result> SaveSettingAsync(SavePomodoroSettingRequest request)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty())
                {
                    _logger.Debug(err);
                    return Result.Error(Result.DATA_INVALID.Code, err);
                }



                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Đã có lỗi xảy ra: {ex.Message}");
                return Result.Exception("Đã có lỗi xảy ra", ex);
            }
        }
    }
}
