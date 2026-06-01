namespace ManageLife.Models
{
    public class CodeSequenceModel
    {
        public string Id { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string Prefix { get; set; } = default!;
        public string? Suffix { get; set; }
        public int NumberLength { get; set; }
        public long CurrentSeq { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
