namespace ManageLife.Settings
{
    public class CronJobOptions
    {
        public const string Section = "CronJob";
        public string ApiKey { get; set; } = null!;
        public string BaseUrl { get; set; } = "https://api.cron-job.org";
        public string WebhookSecret { get; set; } = null!;
        /// <summary>
        /// URL gốc công khai của app (vd https://managelife.runasp.net). Bắt buộc để gắn X-Cron-Secret / đồng bộ job hệ thống:
        /// secret chỉ được gửi tới URL bắt đầu bằng địa chỉ này (không tin Host của request vì client tự đặt được).
        /// </summary>
        public string? AppBaseUrl { get; set; }
    }
}
