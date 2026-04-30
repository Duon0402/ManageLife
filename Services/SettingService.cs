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

        public Task<Result> CreateSettingAsync(CreateSettingRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Result> DeleteSettingAsync(DeleteSettingRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<SettingModel>>> GetListSettingsAsync(GetListSettingsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Result<SettingModel>> GetSettingByIdAsync(GetSettingByIdRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Result<SettingModel>> GetSettingByKeyAsync(GetSettingByKeyRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task RegisterSettingsAsync(List<SettingModel> settings)
        {
            try
            {
                await Task.CompletedTask;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<Result> UpdateSettingAsync(UpdateSettingRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
