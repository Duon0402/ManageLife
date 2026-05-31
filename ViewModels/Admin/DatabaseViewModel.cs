namespace ManageLife.ViewModels
{
    public class DatabaseViewModel
    {
        public List<string> Applied { get; set; } = [];
        public List<string> Pending { get; set; } = [];
        public int PendingCount => Pending.Count;
        public bool IsUpToDate => Pending.Count == 0;
    }
}
