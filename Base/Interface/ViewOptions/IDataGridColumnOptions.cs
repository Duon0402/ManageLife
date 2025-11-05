namespace ManageLife.Base
{
    public interface IDataGridColumnOptions
    {
        string? Data { get; set; }
        string? Name { get; set; }
        string? Title { get; set; }
        string? Type { get; set; }
        bool? Visible { get; set; }
        bool? Orderable { get; set; }
        bool? Searchable { get; set; }
        string? Width { get; set; }
        string? ClassName { get; set; }
        string? DefaultContent { get; set; }

        string? Render { get; set; }
        string? CreatedCell { get; set; }

        string Build();

        IDataGridColumnOptions SetData(string data);
        IDataGridColumnOptions SetName(string name);
        IDataGridColumnOptions SetTitle(string title);
        IDataGridColumnOptions SetType(string type);
        IDataGridColumnOptions SetVisible(bool visible);
        IDataGridColumnOptions SetOrderable(bool orderable);
        IDataGridColumnOptions SetSearchable(bool searchable);
        IDataGridColumnOptions SetWidth(string width);
        IDataGridColumnOptions SetClassName(string className);
        IDataGridColumnOptions SetDefaultContent(string defaultContent);
        IDataGridColumnOptions SetRender(string render);
        IDataGridColumnOptions SetCreatedCell(string createdCell);
    }
}
