using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManageLife.Base
{
    public class DataGridColumnDefOptions : IDataGridColumnDefOptions
    {
        public object? Targets { get; set; }
        public bool? Orderable { get; set; }
        public bool? Visible { get; set; }
        public bool? Searchable { get; set; }
        public string? ClassName { get; set; }
        public string? Width { get; set; }
        public string? Render { get; set; }
        public string? CreatedCell { get; set; }
        public string? DefaultContent { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }

        public string Build()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            return JsonSerializer.Serialize(this, options);
        }

        public IDataGridColumnDefOptions SetTargets(int target)
        {
            Targets = target; return this;
        }
        public IDataGridColumnDefOptions SetTargets(string target)
        {
            Targets = target; return this;
        }
        public IDataGridColumnDefOptions SetTargets(params int[] targets)
        {
            Targets = targets; return this;
        }
        public IDataGridColumnDefOptions SetTargets(params string[] targets)
        {
            Targets = targets; return this;
        }

        public IDataGridColumnDefOptions SetOrderable(bool orderable)
        {
            Orderable = orderable; return this;
        }
        public IDataGridColumnDefOptions SetVisible(bool visible)
        {
            Visible = visible; return this;
        }
        public IDataGridColumnDefOptions SetClassName(string className)
        {
            ClassName = className; return this;
        }
        public IDataGridColumnDefOptions SetSearchable(bool searchable)
        {
            Searchable = searchable; return this;
        }
        public IDataGridColumnDefOptions SetDefaultContent(string defaultContent)
        {
            DefaultContent = defaultContent; return this;
        }
        public IDataGridColumnDefOptions SetTitle(string title)
        {
            Title = title; return this;
        }
        public IDataGridColumnDefOptions SetType(string type)
        {
            Type = type; return this;
        }
        public IDataGridColumnDefOptions SetName(string name)
        {
            Name = name; return this;
        }

        public IDataGridColumnDefOptions SetRender(string render)
        {
            Render = render;
            return this;
        }

        public IDataGridColumnDefOptions SetCreatedCell(string createdCell)
        {
            CreatedCell = createdCell;
            return this;
        }
    }
}
