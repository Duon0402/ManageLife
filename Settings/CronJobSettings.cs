namespace ManageLife.Settings
{
    public class CronJobSettings
    {
        public const string Section = "CronJob";
        public string ApiKey { get; set; } = null!;
        public string BaseUrl { get; set; } = "https://api.cron-job.org";
    }
}
