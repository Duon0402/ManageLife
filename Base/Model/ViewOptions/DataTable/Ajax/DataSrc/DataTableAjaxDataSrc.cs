namespace ManageLife.Base
{
    public class DataTableAjaxDataSrc
    {
        public object? Value { get; set; }

        public DataTableAjaxDataSrc(object? value)
        {
            Value = value;
        }

        public static implicit operator DataTableAjaxDataSrc(string value) => new(value);
        public static implicit operator DataTableAjaxDataSrc(DataTableAjaxDataSrcResult value) => new(value);
    }
}
