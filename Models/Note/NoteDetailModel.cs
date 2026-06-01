namespace ManageLife.Models
{
    public class NoteDetailModel : NoteModel
    {
        public List<NoteModel> LinkedNotes { get; set; } = [];
        public List<NoteModel> BacklinkNotes { get; set; } = [];
    }
}
