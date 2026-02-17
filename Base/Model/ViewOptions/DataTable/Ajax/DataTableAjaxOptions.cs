using System.Text.Json.Serialization;

namespace ManageLife.Base
{
    public class DataTableAjaxOptions
    {
        /// <summary>
        /// URL to load data for the table.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// HTTP method to use (GET or POST). Default is GET.
        /// </summary>
        [JsonConverter(typeof(DataTableAjaxTypeConverter))]
        public DataTableAjaxType? Type { get; set; }

        /// <summary>
        /// Data property or manipulation method for table data.
        /// </summary>
        public DataTableAjaxDataSrc? DataSrc { get; set; }

        /// <summary>
        /// Add or modify data submitted to the server upon an Ajax request
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Set the data parameter submission format
        /// </summary>
        [JsonConverter(typeof(DataTableAjaxSubmitAsConverter))]
        public DataTableAjaxSubmitAs SubmitAs { get; set; }

        /// <summary>
        /// Timeout for Ajax request (in milliseconds).
        /// </summary>
        public int? Timeout { get; set; }

        /// <summary>
        /// Custom headers for the Ajax request.
        /// </summary>
        public Dictionary<string, string>? Headers { get; set; }

        /// <summary>
        /// Called just before an Ajax request is made.
        /// </summary>
        public string? BeforeSend { get; set; }

        /// <summary>
        /// Called after data is loaded successfully.
        /// </summary>
        public string? DataFilter { get; set; }

        /// <summary>
        /// Called on Ajax error.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Called when Ajax request completes (after success or error).
        /// </summary>
        public string? Complete { get; set; }
    }
}
