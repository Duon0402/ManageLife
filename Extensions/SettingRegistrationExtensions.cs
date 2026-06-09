using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Extensions
{
    public static class SettingRegistrationExtensions
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

                // --- Maintenance ---
                new() { Key = SettingKeys.Maintenance.Enabled, Value = "false", Type = SettingType.Boolean, Group = "Maintenance", Description = "Bật chế độ bảo trì — chặn truy cập ngoại trừ admin" },
                new() { Key = SettingKeys.Maintenance.Message, Value = "Website đang bảo trì, vui lòng quay lại sau.", Type = SettingType.Text, Group = "Maintenance", Description = "Thông báo hiển thị khi bảo trì" },

                // --- Feature flags ---
                new() { Key = SettingKeys.Feature.EnableRegistration, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Cho phép người dùng mới đăng ký tài khoản" },
                new() { Key = SettingKeys.Feature.EnableChat, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Bật tính năng Chat" },
                new() { Key = SettingKeys.Feature.EnableVocab, Value = "true", Type = SettingType.Boolean, Group = "Feature", Description = "Bật tính năng học Từ vựng" },

                // --- UI ---
                new() { Key = SettingKeys.Ui.PrimaryColor, Value = "#4b49ac", Type = SettingType.Color, Group = "UI", Description = "Màu chủ đạo của giao diện" },
                new() { Key = SettingKeys.Ui.MaxUploadSizeMb, Value = "10", Type = SettingType.Number, Group = "UI", Description = "Dung lượng tối đa cho phép upload (MB)" },
            };

            await settingService.RegisterSettingsAsync(registered);
        }
    }
}
