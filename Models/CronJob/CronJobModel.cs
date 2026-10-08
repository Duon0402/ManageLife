namespace ManageLife.Models.CronJob
{
    public class CronJobModel
    {
        public int JobId { get; set; }
        public string Title { get; set; } = null!;
        public string Url { get; set; } = null!;
        public bool Enabled { get; set; }
        public bool SaveResponses { get; set; }
        /// <summary>0 GET, 1 POST, 2 OPTIONS, 3 HEAD, 4 PUT, 5 DELETE, 6 TRACE, 7 CONNECT, 8 PATCH.</summary>
        public int RequestMethod { get; set; }
        public int LastStatus { get; set; }
        public int LastDuration { get; set; }
        /// <summary>Unix time (giây).</summary>
        public long? LastExecution { get; set; }
        /// <summary>Unix time (giây).</summary>
        public long? NextExecution { get; set; }
        public CronJobSchedule? Schedule { get; set; }
    }

    public class CronJobDetailModel : CronJobModel
    {
        public CronJobExtendedData? ExtendedData { get; set; }
    }

    /// <summary>Lịch chạy; mỗi danh sách chứa -1 nghĩa là "mọi".</summary>
    public class CronJobSchedule
    {
        public const string DefaultTimezone = "Asia/Ho_Chi_Minh";

        public string Timezone { get; set; } = DefaultTimezone;
        /// <summary>Unix time hết hạn; 0 = không hết hạn.</summary>
        public long ExpiresAt { get; set; }
        public List<int> Hours { get; set; } = new() { -1 };
        public List<int> Minutes { get; set; } = new() { -1 };
        public List<int> Mdays { get; set; } = new() { -1 };
        public List<int> Months { get; set; } = new() { -1 };
        public List<int> Wdays { get; set; } = new() { -1 };
    }
}
