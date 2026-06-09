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
        }

        public static class Ui
        {
            public const string PrimaryColor = "ui.primary_color";
            public const string MaxUploadSizeMb = "ui.max_upload_size_mb";
        }
    }
}
