namespace ManageLife.Base
{
    public class DataTableAjaxTypeConverter : EnumToStringJsonConverter<DataTableAjaxType>
    {
        public DataTableAjaxTypeConverter() : base(new Dictionary<DataTableAjaxType, string>
        {
            { DataTableAjaxType.GET, "GET" },
            { DataTableAjaxType.POST, "POST" },
        })
        { }
    }
}
