namespace ManageLife.Models
{
    public class EmailDailyReportModel
    {
        public List<string> EmailCc { get; set; } = null!;
        public List<string> EmailTo { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
    }
}
