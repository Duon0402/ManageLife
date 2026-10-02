using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class AppConfigService : ServiceBase<AppConfigService>, IAppConfigService
    {
        // Key tính năng của app mobile -> setting bật/tắt. Chỉ khai báo tính năng đã có trên app.
        private static readonly Dictionary<string, string> FeatureFlags = new()
        {
            ["todo"] = SettingKeys.Feature.EnableTodo,
        };

        private readonly ISettingContext _settingContext;

        public AppConfigService(IAppLogger<AppConfigService> logger, IUserContext userContext, ISettingContext settingContext)
            : base(logger, userContext)
        {
            _settingContext = settingContext;
        }

        public async Task<Result<AppConfigModel>> GetConfigAsync(CancellationToken ct = default)
        {
            try
            {
                var model = new AppConfigModel();
                foreach (var (feature, settingKey) in FeatureFlags)
                    model.Features[feature] = await _settingContext.GetBoolAsync(settingKey, true);

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, TranslationKey.Common.Message.SystemError);
                return Result.Exception<AppConfigModel>(TranslationKey.Common.Message.SystemError, ex);
            }
        }
    }
}
