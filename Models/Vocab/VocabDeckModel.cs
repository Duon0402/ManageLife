namespace ManageLife.Models
{
    public class VocabDeckModel
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? TopicId { get; set; }
        public string? TopicName { get; set; }
        public string? TopicColor { get; set; }
        public int TotalCards { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
