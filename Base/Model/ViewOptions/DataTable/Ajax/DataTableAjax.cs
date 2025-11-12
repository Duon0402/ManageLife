namespace ManageLife.Base
{
    public class DataTableAjax
    {
        public object? Value { get; set; }

        public DataTableAjax(object? value)
        {
            Value = value;
        }

        public static implicit operator DataTableAjax(string value) => new(value);
        public static implicit operator DataTableAjax(DataTableAjaxOptions value) => new(value);
    }
}
