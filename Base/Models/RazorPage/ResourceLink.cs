namespace ManageLife.Base
{
    public class ResourceLink
    {
        public string Url { get; set; } = string.Empty;
        public bool IsCdn { get; set; } = false;
        public bool AppendVersion { get; set; } = false;

        public ResourceLink(string url)
        {
            Url = url;
            IsCdn = url.StartsWith("http://") || url.StartsWith("https://");
            AppendVersion = !IsCdn;
        }

        public ResourceLink(string url, bool isCdn)
        {
            Url = url;
            IsCdn = isCdn;
            AppendVersion = !IsCdn;
        }

        public ResourceLink(string url, bool isCdn, bool appendVersion)
        {
            Url = url;
            IsCdn = isCdn;
            AppendVersion = appendVersion;
        }
    }
}
