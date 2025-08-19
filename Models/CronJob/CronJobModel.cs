namespace ManageLife.Models
{
    public class CronJobModel
    {
        public int JobId { get; set; }
        public string Title { get; set; } = null!;
        public string Url { get; set; } = null!;
        public bool Enabled { get; set; }
        public int Type { get; set; }
        public int RequestTimeout { get; set; }
        public bool SaveResponses { get; set; }
        public int LastStatus { get; set; }
        public int LastDuration { get; set; }
        public DateTimeOffset? LastExecution { get; set; }
        public DateTimeOffset? NextExecution { get; set; }
        public string? Timezone { get; set; }
    }
}
