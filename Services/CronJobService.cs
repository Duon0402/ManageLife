using ManageLife.ApiClients;
using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class CronJobService : ICronJobService
    {
        private readonly CronJobApiClient _apiClient;

        public CronJobService(CronJobApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<Result<List<CronJobModel>>> GetListCronJobsAsync(CancellationToken ct = default)
        {
            var result = await _apiClient.GetJobsAsync(ct);
            if (!result.IsOk())
                return Result.Error<List<CronJobModel>>(result.Code, result.Message, result.ErrorContent);
            return Result.Ok(result.Data?.Jobs ?? []);
        }
    }
}
