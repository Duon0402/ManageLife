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

            await settingService.RegisterSettingsAsync(registered);
        }
    }
}
