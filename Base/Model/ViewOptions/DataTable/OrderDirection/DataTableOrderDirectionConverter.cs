namespace ManageLife.Base
{
    public class DataTableOrderDirectionConverter : EnumToStringJsonConverter<DataTableOrderDirection>
    {
        public DataTableOrderDirectionConverter() : base(new Dictionary<DataTableOrderDirection, string>
        {
            { DataTableOrderDirection.None, "" },
            { DataTableOrderDirection.Asc, "asc" },
            { DataTableOrderDirection.Desc, "desc" },
        })
        { }
    }
}
