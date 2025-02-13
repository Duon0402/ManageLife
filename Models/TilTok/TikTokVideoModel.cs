namespace ManageLife.Models
{
    public class TikTokVideoModel
    {
        public string Id { get; set; }
        public long CreateTime { get; set; }
        public string CoverImageUrl { get; set; }
        public string ShareUrl { get; set; }
        public string VideoDescription { get; set; }
        public int Duration { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string Title { get; set; }
        public string EmbedHtml { get; set; }
        public string EmbedLink { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }
        public long ViewCount { get; set; }
    }
}
