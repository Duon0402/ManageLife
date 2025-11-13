namespace ManageLife.Base
{
    public class DataTableOptionsBuilder
    {
        private readonly DataTableOptions _dataTableOptions = new();

        public DataTableOptions Build()
        {
            return _dataTableOptions;
        }

        public static implicit operator DataTableOptions(DataTableOptionsBuilder builder)
        {
            return builder.Build();
        }
    }
}
