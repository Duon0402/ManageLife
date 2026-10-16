using System.Linq;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Extensions
{
    public static class SettingSeedingExtensions
    {
        public static async Task RegisterSettingsAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var pending = await db.Database.GetPendingMigrationsAsync();
            if (pending.Any()) return; // DB chưa migrate — bỏ qua, tránh crash

            var settingService = scope.ServiceProvider.GetRequiredService<ISettingService>();

            var registered = new List<SettingModel>
            {
                // --- Site ---
                new() { Key = SettingKeys.Site.Name, Value = "Manage Life", Type = SettingType.Text, Group = "Site", Description = "Tên website hiển thị trên trình duyệt và header" },
                new() { Key = SettingKeys.Site.Description, Value = "", Type = SettingType.Text, Group = "Site", Description = "Mô tả ngắn về website (dùng cho SEO)" },
                new() { Key = SettingKeys.Site.LogoUrl, Value = "", Type = SettingType.Url, Group = "Site", Description = "URL ảnh logo (để trống dùng logo mặc định)" },
                new() { Key = SettingKeys.Site.FaviconUrl, Value = "", Type = SettingType.Url, Group = "Site", Description = "URL favicon" },
                new() { Key = SettingKeys.Site.FooterText, Value = "", Type = SettingType.Text, Group = "Site", Description = "Nội dung footer (ví dụ: © 2025 Manage Life)" },
                new() { Key = SettingKeys.Site.ContactEmail, Value = "", Type = SettingType.Text, Group = "Site", Description = "Email liên hệ hiển thị trên website" },
                new() { Key = SettingKeys.Site.Hotline, Value = "", Type = SettingType.Text, Group = "Site", Description = "Số điện thoại hotline hiển thị trên website" },

                // --- Maintenance ---
                new() { Key = SettingKeys.Maintenance.Enabled, Value = "false", Type = SettingType.Boolean, Group = "Maintenance", Description = "Bật chế độ bảo trì — chặn truy cập ngoại trừ admin" },
                new() { Key = SettingKeys.Maintenance.Message, Value = "Website đang bảo trì, vui lòng quay lại sau.", Type = SettingType.Text, Group = "Maintenance", Description = "Thông báo hiển thị khi bảo trì" },

                // --- Feature flags ---
                new() { Key = SettingKeys.Feature.EnableRegistration, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Cho phép người dùng mới đăng ký tài khoản" },
                new() { Key = SettingKeys.Feature.EnableChat, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Bật tính năng Chat" },
                new() { Key = SettingKeys.Feature.EnableVocab, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Bật tính năng học Từ vựng" },
                new() { Key = SettingKeys.Feature.EnablePomodoro, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Bật tính năng Pomodoro" },
                new() { Key = SettingKeys.Feature.EnableAnkiCard, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Bật tính năng nhập thẻ + xuất file Anki" },
                new() { Key = SettingKeys.Feature.EnableTodo, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Bật tính năng Công việc (Todo)" },
                new() { Key = SettingKeys.Feature.EnableFinance, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Bật tính năng Tài chính trên app" },
                new() { Key = SettingKeys.Feature.EnableFolder, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Bật tính năng Photo Albums" },
                new() { Key = SettingKeys.Feature.EnableNote, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Bật tính năng Notes" },
                new() { Key = SettingKeys.Feature.EnableHabit, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Bật tính năng Thói quen (Habit)" },
                new() { Key = SettingKeys.Feature.EnableShortUrl, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Bật tính năng URL Shortener" },
                new() { Key = SettingKeys.Feature.EnableVideoDownloader, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Bật tính năng Video Downloader" },
                new() { Key = SettingKeys.Feature.EnableEmailDailyReport, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Bật tính năng gửi báo cáo Email hàng ngày" },

                // --- Todo: từ khoá thêm nhanh (app ghép thành mẫu nhận dạng; để trống = dùng mặc định của app) ---
                new() { Key = SettingKeys.TodoQuickAdd.Today, Value = "hôm nay", Type = SettingType.Text, Group = "Todo", Description = "Thêm nhanh — từ chỉ hôm nay, cách nhau bằng dấu phẩy" },
                new() { Key = SettingKeys.TodoQuickAdd.Tomorrow, Value = "ngày mai, mai", Type = SettingType.Text, Group = "Todo", Description = "Thêm nhanh — từ chỉ ngày mai. Từ đơn (vd \"mai\") chỉ nhận khi gõ chữ thường để không nhầm tên riêng" },
                new() { Key = SettingKeys.TodoQuickAdd.DayAfter, Value = "ngày kia, ngày mốt, mốt", Type = SettingType.Text, Group = "Todo", Description = "Thêm nhanh — từ chỉ ngày kia (sau 2 ngày)" },
                new() { Key = SettingKeys.TodoQuickAdd.NextWeek, Value = "tuần sau, tuần tới", Type = SettingType.Text, Group = "Todo", Description = "Thêm nhanh — từ chỉ tuần sau (đứng riêng = sau 7 ngày; kèm thứ = thứ đó tuần sau)" },
                new() { Key = SettingKeys.TodoQuickAdd.PriorityHigh, Value = "!cao", Type = SettingType.Text, Group = "Todo", Description = "Thêm nhanh — từ đặt ưu tiên cao (vd !cao, !gấp)" },
                new() { Key = SettingKeys.TodoQuickAdd.PriorityMedium, Value = "!vừa, !vua", Type = SettingType.Text, Group = "Todo", Description = "Thêm nhanh — từ đặt ưu tiên vừa" },
                new() { Key = SettingKeys.TodoQuickAdd.PriorityLow, Value = "!thấp, !thap", Type = SettingType.Text, Group = "Todo", Description = "Thêm nhanh — từ đặt ưu tiên thấp" },

                // --- Tài chính: từ khoá ghi nhanh (app so khớp không dấu với tên danh mục; để trống = dùng mặc định của app) ---
                new() { Key = SettingKeys.FinanceQuickAdd.Food, Value = "ăn, cơm, phở, bún, cafe, cà phê, trà sữa", Type = SettingType.Text, Group = "Finance", Description = "Ghi nhanh — từ chỉ danh mục Ăn uống" },
                new() { Key = SettingKeys.FinanceQuickAdd.Transport, Value = "xăng, grab, taxi, gửi xe, bus", Type = SettingType.Text, Group = "Finance", Description = "Ghi nhanh — từ chỉ danh mục Đi lại" },
                new() { Key = SettingKeys.FinanceQuickAdd.Housing, Value = "tiền nhà, tiền thuê nhà", Type = SettingType.Text, Group = "Finance", Description = "Ghi nhanh — từ chỉ danh mục Nhà ở" },
                new() { Key = SettingKeys.FinanceQuickAdd.Bills, Value = "điện, nước, internet, điện thoại", Type = SettingType.Text, Group = "Finance", Description = "Ghi nhanh — từ chỉ danh mục Hoá đơn" },
                new() { Key = SettingKeys.FinanceQuickAdd.Shopping, Value = "mua, shopee, lazada, quần áo", Type = SettingType.Text, Group = "Finance", Description = "Ghi nhanh — từ chỉ danh mục Mua sắm" },
                new() { Key = SettingKeys.FinanceQuickAdd.Fun, Value = "xem phim, karaoke, game", Type = SettingType.Text, Group = "Finance", Description = "Ghi nhanh — từ chỉ danh mục Giải trí" },
                new() { Key = SettingKeys.FinanceQuickAdd.Health, Value = "thuốc, khám", Type = SettingType.Text, Group = "Finance", Description = "Ghi nhanh — từ chỉ danh mục Sức khoẻ" },
                new() { Key = SettingKeys.FinanceQuickAdd.Education, Value = "học, sách", Type = SettingType.Text, Group = "Finance", Description = "Ghi nhanh — từ chỉ danh mục Giáo dục" },
                new() { Key = SettingKeys.FinanceQuickAdd.Family, Value = "con, bố mẹ, gia đình", Type = SettingType.Text, Group = "Finance", Description = "Ghi nhanh — từ chỉ danh mục Gia đình" },
                new() { Key = SettingKeys.FinanceQuickAdd.OtherExpense, Value = "", Type = SettingType.Text, Group = "Finance", Description = "Ghi nhanh — từ chỉ danh mục Chi khác" },
                new() { Key = SettingKeys.FinanceQuickAdd.Salary, Value = "lương", Type = SettingType.Text, Group = "Finance", Description = "Ghi nhanh — từ chỉ danh mục Lương" },
                new() { Key = SettingKeys.FinanceQuickAdd.Bonus, Value = "thưởng", Type = SettingType.Text, Group = "Finance", Description = "Ghi nhanh — từ chỉ danh mục Thưởng" },
                new() { Key = SettingKeys.FinanceQuickAdd.Interest, Value = "lãi", Type = SettingType.Text, Group = "Finance", Description = "Ghi nhanh — từ chỉ danh mục Lãi" },
                new() { Key = SettingKeys.FinanceQuickAdd.OtherIncome, Value = "", Type = SettingType.Text, Group = "Finance", Description = "Ghi nhanh — từ chỉ danh mục Thu khác" },

                // --- UI ---
                new() { Key = SettingKeys.Ui.PrimaryColor, Value = "#4b49ac", Type = SettingType.Color, Group = "UI", Description = "Màu chủ đạo của giao diện" },
                new() { Key = SettingKeys.Ui.MaxUploadSizeMb, Value = "10", Type = SettingType.Number, Group = "UI", Description = "Dung lượng tối đa cho phép upload (MB)" },

                // --- SEO ---
                new() { Key = SettingKeys.Seo.MetaKeywords, Value = "", Type = SettingType.Text, Group = "SEO", Description = "Từ khoá SEO (phân cách bằng dấu phẩy)" },
                new() { Key = SettingKeys.Seo.GoogleAnalyticsId, Value = "", Type = SettingType.Text, Group = "SEO", Description = "Google Analytics Measurement ID (vd: G-XXXXXXX)" },

                // --- Social ---
                new() { Key = SettingKeys.Social.FacebookUrl, Value = "", Type = SettingType.Url, Group = "Social", Description = "Link Fanpage Facebook" },
                new() { Key = SettingKeys.Social.ZaloUrl, Value = "", Type = SettingType.Url, Group = "Social", Description = "Link Zalo liên hệ" },

                // --- Email ---
                new() { Key = SettingKeys.Email.SmtpHost, Value = "", Type = SettingType.Text, Group = "Email", Description = "SMTP host (vd: smtp.gmail.com)" },
                new() { Key = SettingKeys.Email.SmtpPort, Value = "587", Type = SettingType.Number, Group = "Email", Description = "SMTP port" },
                new() { Key = SettingKeys.Email.SmtpUsername, Value = "", Type = SettingType.Text, Group = "Email", Description = "Tài khoản SMTP" },
                new() { Key = SettingKeys.Email.SmtpPassword, Value = "", Type = SettingType.Password, Group = "Email", Description = "Mật khẩu / App password SMTP" },
                new() { Key = SettingKeys.Email.SmtpEnableSsl, Value = "true", Type = SettingType.Boolean, Group = "Email", Description = "Bật SSL/TLS khi gửi mail" },
                new() { Key = SettingKeys.Email.MailFrom, Value = "", Type = SettingType.Text, Group = "Email", Description = "Địa chỉ email gửi đi" },
                new() { Key = SettingKeys.Email.MailFromName, Value = "Manage Life", Type = SettingType.Text, Group = "Email", Description = "Tên hiển thị người gửi" },

                // --- Security ---
                new() { Key = SettingKeys.Security.MaxLoginAttempts, Value = "5", Type = SettingType.Number, Group = "Security", Description = "Số lần đăng nhập sai tối đa trước khi khoá tài khoản" },
                new() { Key = SettingKeys.Security.LockoutMinutes, Value = "15", Type = SettingType.Number, Group = "Security", Description = "Thời gian khoá tài khoản (phút) sau khi đăng nhập sai quá số lần cho phép" },
                new() { Key = SettingKeys.Security.SessionTimeoutMinutes, Value = "60", Type = SettingType.Number, Group = "Security", Description = "Thời gian hết hạn phiên đăng nhập (phút)" },
            };

            // Bộ seed từ khoá ghi nhanh thu chi phải đủ theo danh mục mặc định: thiếu/ thuờng là app không có
            // từ khoá cho danh mục đó mà KHÔNG có lỗi nào báo — kiểm tại chỗ để fail fast lúc khởi động.
            var financeSeeds = registered.Where(item => item.Group == "Finance").Select(item => item.Key).ToHashSet();
            var expected = FinanceDefaults.QuickAddSettingKeys.Values.ToHashSet();
            if (!financeSeeds.SetEquals(expected))
                throw new InvalidOperationException(
                    "Seed từ khoá ghi nhanh thu chi không khớp danh mục mặc định (FinanceDefaults): "
                    + $"thiếu {string.Join(", ", expected.Except(financeSeeds))}; thừa {string.Join(", ", financeSeeds.Except(expected))}");

            await settingService.RegisterSettingsAsync(registered);
        }
    }
}
