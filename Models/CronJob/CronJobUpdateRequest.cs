namespace ManageLife.Models
{
    public class CronJobUpdateRequest
    {
        public string? Title { get; set; }
        public string? Url { get; set; }
        public bool? Enabled { get; set; }
        public int? Type { get; set; }
        public int? RequestTimeout { get; set; }
        public bool? SaveResponses { get; set; }
        public string? Timezone { get; set; }
    }
}
