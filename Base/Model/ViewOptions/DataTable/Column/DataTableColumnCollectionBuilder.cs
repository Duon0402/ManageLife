namespace ManageLife.Base
{
    public class DataTableColumnCollectionBuilder
    {
        private readonly List<DataTableColumnOptions> _columns;

        public DataTableColumnCollectionBuilder(List<DataTableColumnOptions> columns)
        {
            _columns = columns;
        }
    }
}
