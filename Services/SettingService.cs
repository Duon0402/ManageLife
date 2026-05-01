using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class SettingService : ISettingService
    {
        private readonly ISettingRepository _repo;

        public SettingService(ISettingRepository repo)
        {
            _repo = repo;
        }

        public Task<Result> CreateSettingAsync(CreateSettingRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result> DeleteSettingAsync(DeleteSettingRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<SettingModel>>> GetListSettingsAsync(GetListSettingsRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result<SettingModel>> GetSettingByIdAsync(GetSettingByIdRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result<SettingModel>> GetSettingByKeyAsync(GetSettingByKeyRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task RegisterSettingsAsync(List<SettingModel> settings, CancellationToken ct = default)
        {
            await Task.CompletedTask;
        }

        public Task<Result> UpdateSettingAsync(UpdateSettingRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
