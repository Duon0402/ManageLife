namespace ManageLife.Base
{
    public class AjaxOptions
    {
        public string ContentType { get; set; } = "application/json";

        public string DataType { get; set; } = "json";

        public bool ProcessData { get; set; } = true;

        public bool ShowLoading { get; set; } = true;

        public bool ShowToast { get; set; } = true;

        public string? BeforeSend { get; set; }

        public string? OnProgress { get; set; }

        public string? OnSuccess { get; set; }

        public string? OnError { get; set; }

        public string? OnComplete { get; set; }
    }
}
