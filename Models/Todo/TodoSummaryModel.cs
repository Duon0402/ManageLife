namespace ManageLife.Models
{
    public class TodoSummaryModel
    {
        /// <summary>Task chưa xong, hạn đúng hôm nay (giờ VN).</summary>
        public int Today { get; set; }

        /// <summary>Task chưa xong, quá hạn.</summary>
        public int Overdue { get; set; }

        /// <summary>Task hoàn thành trong hôm nay (giờ VN).</summary>
        public int CompletedToday { get; set; }
    }
}
