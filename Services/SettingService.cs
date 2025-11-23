using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Repositories;

namespace ManageLife.Services
{
    public class SettingService : ServiceBase, ISettingService
    {
        private readonly SettingRepository _repo;

        public SettingService(AppDbContext context) : base(context)
        {
            _repo = new SettingRepository(context);
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
            bool b;
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
