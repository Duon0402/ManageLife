namespace ManageLife.Base
{
    public interface IDataGridColumnDefOptions
    {
        object? Targets { get; set; }
        bool? Orderable { get; set; }
        bool? Visible { get; set; }
        bool? Searchable { get; set; }
        string? ClassName { get; set; }
        string? Width { get; set; }
        string? Render { get; set; }
        string? CreatedCell { get; set; }
        string? DefaultContent { get; set; }
        string? Title { get; set; }
        string? Type { get; set; }
        string? Name { get; set; }

        string Build();

        IDataGridColumnDefOptions SetTargets(int target);
        IDataGridColumnDefOptions SetTargets(string target);
        IDataGridColumnDefOptions SetTargets(params int[] targets);
        IDataGridColumnDefOptions SetTargets(params string[] targets);
        IDataGridColumnDefOptions SetOrderable(bool orderable);
        IDataGridColumnDefOptions SetVisible(bool visible);
        IDataGridColumnDefOptions SetSearchable(bool searchable);
        IDataGridColumnDefOptions SetClassName(string className);
        IDataGridColumnDefOptions SetRender(string render);
        IDataGridColumnDefOptions SetCreatedCell(string createdCell);
        IDataGridColumnDefOptions SetDefaultContent(string defaultContent);
        IDataGridColumnDefOptions SetTitle(string title);
        IDataGridColumnDefOptions SetType(string type);
        IDataGridColumnDefOptions SetName(string name);
    }
}
