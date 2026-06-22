namespace ManageLife.Models
{
    public class PomodoroHistoryModel
    {
        public List<PomodoroSessionModel> Sessions { get; set; } = [];
        public int TotalFocusMinutes { get; set; }
        public int CompletedFocusSessions { get; set; }
    }
}
