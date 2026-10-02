using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IAppConfigService
    {
        Task<Result<AppConfigModel>> GetConfigAsync(CancellationToken ct = default);
    }
}
