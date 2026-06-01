namespace ManageLife.Models
{
    public class NoteModel
    {
        public string Id { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? Content { get; set; }
        public List<NoteTagModel> Tags { get; set; } = [];
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
