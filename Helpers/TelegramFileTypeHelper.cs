using ManageLife.Commons;

namespace ManageLife.Helpers
{
    public static class TelegramFileTypeHelper
    {
        public static TelegramFileType Detect(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return TelegramFileType.Unknown;

            var extension = Path.GetExtension(file.FileName)?.ToLower();
            var mime = file.ContentType?.ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(mime))
                return TelegramFileType.Unknown;

            if (extension == ".gif" || mime == "image/gif")
                return TelegramFileType.Animation;

            if (mime.StartsWith("image/"))
                return TelegramFileType.Photo;

            if (mime.StartsWith("video/"))
                return TelegramFileType.Video;

            if (mime.StartsWith("audio/"))
                return TelegramFileType.Audio;

            return TelegramFileType.Document;
        }
    }
}
