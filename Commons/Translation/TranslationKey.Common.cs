namespace ManageLife.Commons.Translation
{
    public partial class TranslationKey
    {
        public class Common
        {
            public class Button
            {
                public const string Create = "TranslationKey.Common.Button.Create";
                public const string Update = "TranslationKey.Common.Button.Update";
                public const string Delete = "TranslationKey.Common.Button.Delete";
            }

            public class Message
            {
                public const string CreateError = "TranslationKey.Common.Message.CreateError"; // Thêm mới {0} không thành công
                public const string UpdateError = "TranslationKey.Common.Message.UpdateError";
                public const string DeleteError = "TranslationKey.Common.Message.DeleteError";

                public const string DataNotExisted = "TranslationKey.Common.Message.DataNotExisted";
                public const string DataExisted = "TranslationKey.Common.Message.DataExisted";

                public const string InvalidData = "TranslationKey.Common.Message.InvalidData";
                public const string SystemError = "TranslationKey.Common.Message.SystemError";
            }
        }
    }
}
