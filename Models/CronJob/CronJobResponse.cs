namespace ManageLife.Models.CronJob
{
    // DTO theo REST API của cron-job.org (https://docs.cron-job.org/rest-api.html)

    public class CronJobResponse
    {
        public List<CronJobModel> Jobs { get; set; } = new();
    }

    public class CronJobDetailResponse
    {
        public CronJobDetailModel JobDetails { get; set; } = null!;
    }

    public class CronJobCreateResponse
    {
        public int JobId { get; set; }
    }

    public class CronJobHistoryResponse
    {
        public List<CronJobHistoryItem> History { get; set; } = new();
    }

    public class CronJobHistoryItem
    {
        /// <summary>Unix time (giây) lúc chạy.</summary>
        public long Date { get; set; }
        /// <summary>Thời gian chạy (ms).</summary>
        public int Duration { get; set; }
        public int Status { get; set; }
        public string? StatusText { get; set; }
        public int HttpStatus { get; set; }
    }

    /// <summary>Body gửi khi tạo (PUT /jobs) hoặc sửa (PATCH /jobs/{id}).</summary>
    public class CronJobWriteRequest
    {
        public CronJobWriteModel Job { get; set; } = null!;
    }

    public class CronJobWriteModel
    {
        public string Title { get; set; } = null!;
        public string Url { get; set; } = null!;
        public bool Enabled { get; set; }
        public bool SaveResponses { get; set; }
        public int RequestMethod { get; set; }
        public CronJobSchedule Schedule { get; set; } = null!;
        public CronJobExtendedData ExtendedData { get; set; } = new();
    }

    public class CronJobExtendedData
    {
        public Dictionary<string, string> Headers { get; set; } = new();
        //NOTE: Gửi "" (không bỏ trường) để PATCH xoá được body cũ
        public string? Body { get; set; }
    }
}
