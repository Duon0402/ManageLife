namespace ManageLife.Models
{
    public class EmailDailyReportTemplate
    {
        public const string Subject = "[IT] Báo cáo công việc của {0} ngày {1}, kế hoạch ngày {2}";

        public const string Body = @"Kính gửi Ban lãnh đạo,

Tôi là: {0}, mã nhân viên: {1}
Đơn vị: {2}

I. Kết quả công việc ngày {3}
{5}

II. Dự kiến công việc ngày {4}
{6}

III. Ý kiến đề xuất
{7}

Trân trọng,
{0}";
    }
}
