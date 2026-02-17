namespace ManageLife.Base
{
    public class DataTableAjaxSubmitAsConverter : EnumToStringJsonConverter<DataTableAjaxSubmitAs>
    {
        public DataTableAjaxSubmitAsConverter() : base(new Dictionary<DataTableAjaxSubmitAs, string>
        {
            { DataTableAjaxSubmitAs.Http, "http" },
            { DataTableAjaxSubmitAs.Json, "json" },
        })
        { }
    }
}
