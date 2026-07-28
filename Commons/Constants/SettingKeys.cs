namespace ManageLife.Commons
{
    public static class SettingKeys
    {
        public static class Site
        {
            public const string Name = "site.name";
            public const string Description = "site.description";
            public const string LogoUrl = "site.logo_url";
            public const string FaviconUrl = "site.favicon_url";
            public const string FooterText = "site.footer_text";
            public const string ContactEmail = "site.contact_email";
            public const string Hotline = "site.hotline";
        }

        public static class Maintenance
        {
            public const string Enabled = "maintenance.enabled";
            public const string Message = "maintenance.message";
        }

        public static class Feature
        {
            public const string EnableRegistration = "feature.enable_registration";
            public const string EnableChat = "feature.enable_chat";
            public const string EnableVocab = "feature.enable_vocab";
            public const string EnablePomodoro = "feature.enable_pomodoro";
            public const string EnableAnkiCard = "feature.enable_anki_card";
            public const string EnableTodo = "feature.enable_todo";
            public const string EnableFolder = "feature.enable_folder";
            public const string EnableNote = "feature.enable_note";
            public const string EnableHabit = "feature.enable_habit";
            public const string EnableShortUrl = "feature.enable_short_url";
            public const string EnableVideoDownloader = "feature.enable_video_downloader";
            public const string EnableEmailDailyReport = "feature.enable_email_daily_report";
        }

        public static class Ui
        {
            public const string PrimaryColor = "ui.primary_color";
            public const string MaxUploadSizeMb = "ui.max_upload_size_mb";
        }

        public static class Seo
        {
            public const string MetaKeywords = "seo.meta_keywords";
            public const string GoogleAnalyticsId = "seo.google_analytics_id";
        }

        public static class Social
        {
            public const string FacebookUrl = "social.facebook_url";
            public const string ZaloUrl = "social.zalo_url";
        }

        public static class Email
        {
            public const string SmtpHost = "email.smtp_host";
            public const string SmtpPort = "email.smtp_port";
            public const string SmtpUsername = "email.smtp_username";
            public const string SmtpPassword = "email.smtp_password";
            public const string SmtpEnableSsl = "email.smtp_enable_ssl";
            public const string MailFrom = "email.mail_from";
            public const string MailFromName = "email.mail_from_name";
        }

        public static class Security
        {
            public const string MaxLoginAttempts = "security.max_login_attempts";
            public const string LockoutMinutes = "security.lockout_minutes";
            public const string SessionTimeoutMinutes = "security.session_timeout_minutes";
        }
    }
}
