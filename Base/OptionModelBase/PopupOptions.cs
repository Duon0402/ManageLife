using Microsoft.AspNetCore.Html;

namespace ManageLife.Base
{
    public class PopupOptions
    {
        public string Id { get; set; } = "popup_id";

        // Title
        public string Title { get; set; } = string.Empty;
        public bool ShowTitle { get; set; } = true;

        // Width
        public string? Width { get; set; }
        public string? MinWidth { get; set; }

        // Height
        public string? Height { get; set; }
        public string? MinHeight { get; set; }

        // Container
        public IHtmlContent? Content { get; set; }

        // Close Button
        public bool ShowCloseButton { get; set; } = true;

        // Toolbar Items
    }

    public class ToolBarItems
    {
        public List<ToolBarItem> Items = new List<ToolBarItem>();
    }

    public class ToolBarItem
    {
        public string Text { get; set; } = string.Empty;
        public string CssClass { get; set; } = string.Empty;
        public string OnClickFUnction { get; set; } = string.Empty;
    }
}
