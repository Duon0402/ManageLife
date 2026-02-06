using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ISettingService
    {
        Task<Result<List<SettingModel>>> GetListSettingsAsync(GetListSettingsRequest request);
        Task<Result<SettingModel>> GetSettingByIdAsync(GetSettingByIdRequest request);
        Task<Result<SettingModel>> GetSettingByKeyAsync(GetSettingByKeyRequest request);
        Task<Result> CreateSettingAsync(CreateSettingRequest request);
        Task<Result> UpdateSettingAsync(UpdateSettingRequest request);
        Task<Result> DeleteSettingAsync(DeleteSettingRequest request);
        Task RegisterSettingsAsync(List<SettingModel> settings);
    }
}
