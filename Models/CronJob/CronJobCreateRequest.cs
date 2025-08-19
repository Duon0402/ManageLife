namespace ManageLife.Models
{
    public class CronJobCreateRequest
    {
        public string Title { get; set; } = null!;
        public string Url { get; set; } = null!;
        public bool Enabled { get; set; } = true;
        public int Type { get; set; }
        public int RequestTimeout { get; set; }
        public bool SaveResponses { get; set; }
        public string? Timezone { get; set; }
    }
}
