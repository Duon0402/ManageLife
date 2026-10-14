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
            ["finance"] = SettingKeys.Feature.EnableFinance,
        };

        // Nhóm từ khoá thêm nhanh của app -> setting (key nhóm khớp với app)
        private static readonly Dictionary<string, string> QuickAddKeywords = new()
        {
            ["today"] = SettingKeys.TodoQuickAdd.Today,
            ["tomorrow"] = SettingKeys.TodoQuickAdd.Tomorrow,
            ["dayAfter"] = SettingKeys.TodoQuickAdd.DayAfter,
            ["nextWeek"] = SettingKeys.TodoQuickAdd.NextWeek,
            ["priorityHigh"] = SettingKeys.TodoQuickAdd.PriorityHigh,
            ["priorityMedium"] = SettingKeys.TodoQuickAdd.PriorityMedium,
            ["priorityLow"] = SettingKeys.TodoQuickAdd.PriorityLow,
        };

        // Chặn cấu hình quá lớn (mỗi lần gõ phím app đều so khớp các từ này)
        private const int MaxKeywordsPerGroup = 20;
        private const int MaxKeywordLength = 30;

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

                foreach (var (group, settingKey) in QuickAddKeywords)
                    model.QuickAdd[group] = SplitKeywords(await _settingContext.GetStringAsync(settingKey));

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, TranslationKey.Common.Message.SystemError);
                return Result.Exception<AppConfigModel>(TranslationKey.Common.Message.SystemError, ex);
            }
        }

        public async Task<Result<AppPublicConfigModel>> GetPublicConfigAsync(CancellationToken ct = default)
        {
            try
            {
                var model = new AppPublicConfigModel
                {
                    RegistrationEnabled = await _settingContext.GetBoolAsync(SettingKeys.Feature.EnableRegistration, true)
                };
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, TranslationKey.Common.Message.SystemError);
                return Result.Exception<AppPublicConfigModel>(TranslationKey.Common.Message.SystemError, ex);
            }
        }

        /// <summary>"a, b ,, c" -> ["a","b","c"]: bỏ trống/trùng, gộp khoảng trắng, chuẩn hoá Unicode (NFC) cho app so khớp.</summary>
        private static List<string> SplitKeywords(string? value)
        {
            if (value.IsEmpty()) return new List<string>();
            return value!.Split(',')
                .Select(x => string.Join(' ', x.Split(' ', StringSplitOptions.RemoveEmptyEntries)).Normalize())
                .Where(x => x.IsNotEmpty() && x.Length <= MaxKeywordLength)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaxKeywordsPerGroup)
                .ToList();
        }
    }
}
