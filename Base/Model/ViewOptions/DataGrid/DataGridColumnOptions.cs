using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManageLife.Base
{
    public class DataGridColumnOptions : IDataGridColumnOptions
    {
        public string? Data { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; }
        public bool? Visible { get; set; }
        public bool? Orderable { get; set; }
        public bool? Searchable { get; set; }
        public string? Width { get; set; }
        public string? ClassName { get; set; }
        public string? DefaultContent { get; set; }

        public string? Render { get; set; }
        public string? CreatedCell { get; set; }

        public string Build()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            return JsonSerializer.Serialize(this, options);
        }

        public IDataGridColumnOptions SetData(string data)
        {
            Data = data;
            return this;
        }

        public IDataGridColumnOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        public IDataGridColumnOptions SetTitle(string title)
        {
            Title = title;
            return this;
        }

        public IDataGridColumnOptions SetType(string type)
        {
            Type = type;
            return this;
        }

        public IDataGridColumnOptions SetVisible(bool visible)
        {
            Visible = visible;
            return this;
        }

        public IDataGridColumnOptions SetOrderable(bool orderable)
        {
            Orderable = orderable;
            return this;
        }

        public IDataGridColumnOptions SetSearchable(bool searchable)
        {
            Searchable = searchable;
            return this;
        }

        public IDataGridColumnOptions SetWidth(string width)
        {
            Width = width;
            return this;
        }

        public IDataGridColumnOptions SetClassName(string className)
        {
            ClassName = className;
            return this;
        }

        public IDataGridColumnOptions SetDefaultContent(string defaultContent)
        {
            DefaultContent = defaultContent;
            return this;
        }

        public IDataGridColumnOptions SetRender(string render)
        {
            Render = render;
            return this;
        }

        public IDataGridColumnOptions SetCreatedCell(string createdCell)
        {
            CreatedCell = createdCell;
            return this;
        }
    }
}
