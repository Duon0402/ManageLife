using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IAppConfigService
    {
        Task<Result<AppConfigModel>> GetConfigAsync(CancellationToken ct = default);

        /// <summary>Cấu hình công khai (không cần đăng nhập): cờ đăng ký cho màn đăng nhập.</summary>
        Task<Result<AppPublicConfigModel>> GetPublicConfigAsync(CancellationToken ct = default);
    }
}
