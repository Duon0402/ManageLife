namespace ManageLife.Base.Model.ViewOptions.DataTable.Ajax
{
    public class DataTableAjaxOptionsBuilder
    {
        private readonly DataTableAjaxOptions _ajaxOptions = new();

        public DataTableAjaxOptionsBuilder Url(string url)
        {
            _ajaxOptions.Url = url;
            return this;
        }

        public DataTableAjaxOptionsBuilder Type(DataTableAjaxType type)
        {
            _ajaxOptions.Type = type;
            return this;
        }

        public DataTableAjaxOptionsBuilder Data(object data)
        {
            _ajaxOptions.Data = data;
            return this;
        }

        //public DataTableAjaxOptionsBuilder DataSrc(string property)
        //{
        //    _ajaxOptions.DataSrc = new DataTableAjaxDataSrc { Property = property };
        //    return this;
        //}

        public DataTableAjaxOptionsBuilder DataSrc(DataTableAjaxDataSrc dataSrc)
        {
            _ajaxOptions.DataSrc = dataSrc;
            return this;
        }

        public DataTableAjaxOptionsBuilder SubmitAs(DataTableAjaxSubmitAs submitAs)
        {
            _ajaxOptions.SubmitAs = submitAs;
            return this;
        }

        public DataTableAjaxOptionsBuilder Timeout(int timeout)
        {
            _ajaxOptions.Timeout = timeout;
            return this;
        }

        public DataTableAjaxOptionsBuilder Headers(Dictionary<string, string> headers)
        {
            _ajaxOptions.Headers = headers;
            return this;
        }

        public DataTableAjaxOptionsBuilder Header(string key, string value)
        {
            _ajaxOptions.Headers ??= new Dictionary<string, string>();
            _ajaxOptions.Headers[key] = value;
            return this;
        }

        public DataTableAjaxOptionsBuilder BeforeSend(string beforeSendJs)
        {
            _ajaxOptions.BeforeSend = beforeSendJs;
            return this;
        }

        public DataTableAjaxOptionsBuilder DataFilter(string dataFilterJs)
        {
            _ajaxOptions.DataFilter = dataFilterJs;
            return this;
        }

        public DataTableAjaxOptionsBuilder Error(string errorJs)
        {
            _ajaxOptions.Error = errorJs;
            return this;
        }

        public DataTableAjaxOptionsBuilder Complete(string completeJs)
        {
            _ajaxOptions.Complete = completeJs;
            return this;
        }

        public DataTableAjaxOptions Build()
        {
            return _ajaxOptions;
        }

        public static implicit operator DataTableAjaxOptions(DataTableAjaxOptionsBuilder builder)
        {
            return builder.Build();
        }
    }
}
