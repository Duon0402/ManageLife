using AutoMapper;
using ManageLife.ApiClients;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class CronJobService : ServiceBase<FolderService>, ICronJobService
    {
        private readonly CronJobApiClient _apiClient;

        public CronJobService(IAppLogger<FolderService> logger, IUserContext userContext, IMapper mapper, CronJobApiClient apiClient) : base(logger, userContext, mapper)
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
