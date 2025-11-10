namespace ManageLife.Base
{
    public enum DataTableColumnType
    {
        /// <summary>
        /// Fall back type if the data in the column does not match the requirements for the other data types (above).
        /// </summary>
        String,

        /// <summary>
        /// Date / time values. Note that DataTables' built in date parsing works to an ISO 8601 format with 3 separators (/, - and ,).
        /// Additional date format support can be added through the use of the built in datetime renderer plus one of the Moment.js or Luxon libraries.
        /// </summary>
        Date,

        /// <summary>
        /// Simple number sorting.
        /// </summary>
        Num,

        /// <summary>
        /// Numeric sorting of formatted numbers.
        /// Numbers which are formatted with thousands separators, currency symbols or a percentage indicator will be sorted numerically automatically by DataTables.
        /// </summary>
        NumFmt,

        /// <summary>
        /// As per the num option, but with HTML tags also in the data.
        /// </summary>
        HtmlNum,

        /// <summary>
        /// As per the num-fmt option, but with HTML tags also in the data.
        /// </summary>
        HtmlNumFmt,

        /// <summary>
        /// Detected if the string contains HTML tags and it contains non-ASCII characters
        /// </summary>
        HtmlUtf8,

        /// <summary>
        /// Basic string processing for HTML tags
        /// </summary>
        Html,

        /// <summary>
        /// String data type if the text is found to contain non-ASCII characters
        /// </summary>
        StringUtf8,
    }
}
