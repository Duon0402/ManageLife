using ManageLife.Base;
using ManageLife.Data;

namespace ManageLife.Services
{
    public class CronJobService : ServiceBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public CronJobService(AppDbContext context, IConfiguration config, HttpClient httpClient) : base(context)
        {
            _config = config;
            _apiKey = _config["CronJob:ApiKey"] ?? throw new ArgumentNullException("ApiKey");
            _baseUrl = _config["CronJob:BaseUrl"] ?? "https://api.cron-job.org";

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<Result> CreateCronJobAsync()
        {
            string msg;
            try
            {
                return Result.Ok();

            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
