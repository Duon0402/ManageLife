using System.Text.Json.Serialization;

namespace ManageLife.Models
{
    public class FreeDictionaryResponse
    {
        [JsonPropertyName("word")]
        public string Word { get; set; } = default!;

        [JsonPropertyName("phonetics")]
        public List<FreeDictionaryPhonetic> Phonetics { get; set; } = [];

        [JsonPropertyName("meanings")]
        public List<FreeDictionaryMeaning> Meanings { get; set; } = [];
    }

    public class FreeDictionaryPhonetic
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("audio")]
        public string? Audio { get; set; }
    }

    public class FreeDictionaryMeaning
    {
        [JsonPropertyName("partOfSpeech")]
        public string PartOfSpeech { get; set; } = default!;

        [JsonPropertyName("definitions")]
        public List<FreeDictionaryDefinition> Definitions { get; set; } = [];
    }

    public class FreeDictionaryDefinition
    {
        [JsonPropertyName("definition")]
        public string Definition { get; set; } = default!;

        [JsonPropertyName("example")]
        public string? Example { get; set; }

        [JsonPropertyName("synonyms")]
        public List<string> Synonyms { get; set; } = [];
    }
}
