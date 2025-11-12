namespace ManageLife.Base
{
    public class DataTableAjaxDataSrcResult
    {
        public DataTableAjaxDataSrcResult()
        {
        }

        public DataTableAjaxDataSrcResult(string error)
        {
            Draw = 0;
            RecordsTotal = 0;
            RecordsFiltered = 0;
            Data = new List<object>();
            Error = error;
        }

        /// <summary>
        /// The draw counter that this object is a response to
        /// </summary>
        public int Draw { get; set; }

        /// <summary>
        /// Total records, before filtering
        /// </summary>
        public int RecordsTotal { get; set; }

        /// <summary>
        /// Total records, after filtering
        /// </summary>
        public int RecordsFiltered { get; set; }

        /// <summary>
        /// Data to be displayed in the table
        /// </summary>
        public List<object> Data { get; set; } = new();

        /// <summary>
        /// Optional error message
        /// </summary>
        public string? Error { get; set; }
    }
}
