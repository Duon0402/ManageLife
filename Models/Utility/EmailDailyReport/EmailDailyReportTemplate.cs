namespace ManageLife.Models
{
    public class EmailDailyReportTemplate
    {
        public const string Subject = "[IT] Báo cáo công việc của {0} ngày {1}, kế hoạch ngày {2}";
        public const string Body = @"Kính gửi Ban lãnh đạo,

Tôi là: {0}, mã nhân viên: {1}
Đơn vị: {2}

I. Kết quả công việc ngày {3}
- Liệt kê các công việc đã làm, kết quả/tiến độ, lý do chưa hoàn thành. Mã task trên Jira nếu có.

II. Dự kiến công việc ngày {4}
- Liệt kê các công việc sẽ làm. Mã task trên Jira nếu có. Các yêu cầu hỗ trợ hoặc phụ trách nếu có.

III. Ý kiến đề xuất
- Các ý kiến đề xuất nếu có.

Trân trọng,
{0}";
    }
}
