using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ISettingService
    {
        Task<Result<List<SettingModel>>> GetListSettingsAsync(GetListSettingsRequest request, CancellationToken ct = default);
        Task<Result<SettingModel>> GetSettingByIdAsync(GetSettingByIdRequest request, CancellationToken ct = default);
        Task<Result<SettingModel>> GetSettingByKeyAsync(GetSettingByKeyRequest request, CancellationToken ct = default);
        Task<Result> CreateSettingAsync(CreateSettingRequest request, CancellationToken ct = default);
        Task<Result> UpdateSettingAsync(UpdateSettingRequest request, CancellationToken ct = default);
        Task<Result> DeleteSettingAsync(DeleteSettingRequest request, CancellationToken ct = default);
        Task RegisterSettingsAsync(List<SettingModel> settings, CancellationToken ct = default);
    }
}
