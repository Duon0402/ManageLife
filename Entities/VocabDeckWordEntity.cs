namespace ManageLife.Entities
{
    public class VocabDeckWordEntity
    {
        public string DeckId { get; set; } = default!;
        public string WordId { get; set; } = default!;
        public int SortOrder { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
