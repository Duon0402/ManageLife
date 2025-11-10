namespace ManageLife.Base
{
    public class DataTableColumnCellTypeConverter : EnumToStringJsonConverter<DataTableColumnCellType>
    {
        public DataTableColumnCellTypeConverter() : base(new Dictionary<DataTableColumnCellType, string>
        {
            { DataTableColumnCellType.Td, "td" },
            { DataTableColumnCellType.Th, "th" },
        })
        { }
    }
}
