namespace ManageLife.Core.Models
{
    public class VideoInfo
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AuthorNickname { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string MusicUrl { get; set; } = string.Empty;
        public string MusicTitle { get; set; } = string.Empty;
        public int Duration { get; set; }
    }
}
