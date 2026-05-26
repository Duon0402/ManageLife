using ManageLife.Core;
using ManageLife.Core.Http;
using ManageLife.Models.CronJob;

namespace ManageLife.ApiClients
{
    public class CronJobApiClient : BaseHttpApiClient
    {
        public CronJobApiClient(HttpClient http) : base(http) { }

        public Task<Result<CronJobResponse>> GetJobsAsync(CancellationToken ct = default)
            => GetAsync<CronJobResponse>("/jobs", ct);
    }
}
