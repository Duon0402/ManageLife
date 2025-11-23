namespace ManageLife.Base
{
    public class DataTableColumnTypeConverter : EnumToStringJsonConverter<DataTableColumnType>
    {
        public DataTableColumnTypeConverter() : base(new Dictionary<DataTableColumnType, string>
        {
            { DataTableColumnType.Date, "date" },
            { DataTableColumnType.Num, "num" },
            { DataTableColumnType.NumFmt, "num-fmt" },
            { DataTableColumnType.HtmlNum, "html-num" },
            { DataTableColumnType.HtmlNumFmt, "html-num-fmt" },
            { DataTableColumnType.HtmlUtf8, "html-utf8" },
            { DataTableColumnType.Html, "html" },
            { DataTableColumnType.StringUtf8, "string-utf8" },
            { DataTableColumnType.String, "string" }
        })
        { }
    }
}
