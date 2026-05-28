namespace ManageLife.Models
{
    public class GetListVocabWordsRequest
    {
        public string? SearchKeyword { get; set; }
        public int? MasteryLevel { get; set; }
        public string? DeckId { get; set; }
    }
}
