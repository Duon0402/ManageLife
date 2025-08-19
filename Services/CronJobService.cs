using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Models.CronJob;

namespace ManageLife.Services
{
    public class CronJobService : ServiceBase, ICronJobService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public CronJobService(AppDbContext context, IConfiguration config, IHttpClientFactory httpClientFactory) : base(context)
        {
            _config = config;
            _apiKey = _config["CronJob:ApiKey"] ?? throw new ArgumentNullException("ApiKey");
            _baseUrl = _config["CronJob:BaseUrl"] ?? "https://api.cron-job.org";

            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<Result<List<CronJobModel>>> GetListCronJobsAsync()
        {
            try
            {
                var res = await _httpClient.GetAsync("/jobs");

                if (!res.IsSuccessStatusCode)
                {
                    string msg = $"Không thể lấy danh sách cron jobs: {res.ReasonPhrase}";
                    return Result.Error<List<CronJobModel>>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var json = await res.Content.ReadFromJsonAsync<CronJobResponse>();
                var models = json?.Jobs ?? new List<CronJobModel>();

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                string msg = $"Đã xảy ra lỗi khi lấy danh sách cron jobs: {ex.Message}";
                return Result.Exception<List<CronJobModel>>(msg, ex);
            }
        }
    }
}
