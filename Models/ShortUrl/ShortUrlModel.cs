namespace ManageLife.Models
{
    public class ShortUrlModel
    {
        public string Id { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string OriginalUrl { get; set; } = default!;
        public string? Title { get; set; }
        public int ClickCount { get; set; }
        public DateTime? ExpireAt { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
