namespace ManageLife.Models
{
    public class VocabTopicModel
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public bool IsPublic { get; set; }
        public int DeckCount { get; set; }
    }
}
