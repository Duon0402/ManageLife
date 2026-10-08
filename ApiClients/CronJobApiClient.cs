using ManageLife.Core;
using ManageLife.Core.Http;
using ManageLife.Models.CronJob;

namespace ManageLife.ApiClients
{
    /// <summary>REST API của cron-job.org (Bearer CronJob:ApiKey). Có giới hạn số request/ngày nên phía service cache danh sách.</summary>
    public class CronJobApiClient : BaseHttpApiClient
    {
        public CronJobApiClient(HttpClient http) : base(http) { }

        public Task<Result<CronJobResponse>> GetJobsAsync(CancellationToken ct = default)
            => GetAsync<CronJobResponse>("/jobs", ct);

        public Task<Result<CronJobDetailResponse>> GetJobAsync(int jobId, CancellationToken ct = default)
            => GetAsync<CronJobDetailResponse>($"/jobs/{jobId}", ct);

        public Task<Result<CronJobCreateResponse>> CreateJobAsync(CronJobWriteModel job, CancellationToken ct = default)
            => PutAsync<CronJobCreateResponse>("/jobs", new CronJobWriteRequest { Job = job }, ct);

        public Task<Result> UpdateJobAsync(int jobId, CronJobWriteModel job, CancellationToken ct = default)
            => PatchAsync($"/jobs/{jobId}", new CronJobWriteRequest { Job = job }, ct);

        public Task<Result> DeleteJobAsync(int jobId, CancellationToken ct = default)
            => DeleteAsync($"/jobs/{jobId}", ct);

        public Task<Result<CronJobHistoryResponse>> GetHistoryAsync(int jobId, CancellationToken ct = default)
            => GetAsync<CronJobHistoryResponse>($"/jobs/{jobId}/history", ct);
    }
}
