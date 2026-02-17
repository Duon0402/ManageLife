namespace ManageLife.Base
{
    public class DataTableColumnOrderDirectionConverter : EnumToStringJsonConverter<DataTableColumnOrderDirection>
    {
        public DataTableColumnOrderDirectionConverter() : base(new Dictionary<DataTableColumnOrderDirection, string>
        {
            { DataTableColumnOrderDirection.None, "" },
            { DataTableColumnOrderDirection.Asc, "asc" },
            { DataTableColumnOrderDirection.Desc, "desc" },
        })
        { }
    }
}
