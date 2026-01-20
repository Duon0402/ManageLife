namespace ManageLife.Commons
{
    public partial class TranslationKey
    {
        public partial class Client
        {
            public static class EmailDailyReport
            {
                public const string Title = "TranslationKey.Client.EmailDailyReport.Title";

                public static class Section
                {
                    public const string EnterReport = "TranslationKey.Client.EmailDailyReport.Section.EnterReport";

                    public const string EmailTemplate = "TranslationKey.Client.EmailDailyReport.Section.EmailTemplate";
                }

                public static class Field
                {
                    public const string CurrentBusinessDay = "TranslationKey.Client.EmailDailyReport.Field.CurrentBusinessDay";

                    public const string NextBusinessDay = "TranslationKey.Client.EmailDailyReport.Field.NextBusinessDay";

                    public const string TodayWorkResults = "TranslationKey.Client.EmailDailyReport.Field.TodayWorkResults";

                    public const string PlannedWorkTomorrow = "TranslationKey.Client.EmailDailyReport.Field.PlannedWorkTomorrow";

                    public const string Suggestions = "TranslationKey.Client.EmailDailyReport.Field.Suggestions";

                    public const string EmailTo = "TranslationKey.Client.EmailDailyReport.Field.EmailTo";

                    public const string EmailCc = "TranslationKey.Client.EmailDailyReport.Field.EmailCc";

                    public const string Subject = "TranslationKey.Client.EmailDailyReport.Field.Subject";

                    public const string Body = "TranslationKey.Client.EmailDailyReport.Field.Body";
                }

                public static class Button
                {
                    public const string GenerateEmail = "TranslationKey.Client.EmailDailyReport.Button.GenerateEmail";

                    public const string Copy = "TranslationKey.Client.EmailDailyReport.Button.Copy";
                }

                public static class Message
                {
                    public const string CopySuccess = "TranslationKey.Client.EmailDailyReport.Message.CopySuccess";

                    public const string CopyFailed = "TranslationKey.Client.EmailDailyReport.Message.CopyFailed";

                    public const string NoContentToCopy = "TranslationKey.Client.EmailDailyReport.Message.NoContentToCopy";

                    public const string GenerateError = "TranslationKey.Client.EmailDailyReport.Message.GenerateError";
                }
            }
        }
    }
}
