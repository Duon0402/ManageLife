namespace ManageLife.Base
{
    public class DataTableCellTypeConverter : EnumToStringJsonConverter<DataTableCellType>
    {
        public DataTableCellTypeConverter() : base(new Dictionary<DataTableCellType, string>
        {
            { DataTableCellType.Td, "td" },
            { DataTableCellType.Th, "th" },
        })
        { }
    }
}
