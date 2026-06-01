namespace ManageLife.Models
{
    public class NoteGraphModel
    {
        public List<NoteGraphNodeData> Nodes { get; set; } = [];
        public List<NoteGraphEdgeData> Edges { get; set; } = [];
    }

    public class NoteGraphNodeData
    {
        public string Id { get; set; } = default!;
        public string Label { get; set; } = default!;
        public List<string> TagIds { get; set; } = [];
        public int LinkCount { get; set; }
    }

    public class NoteGraphEdgeData
    {
        public string Source { get; set; } = default!;
        public string Target { get; set; } = default!;
    }
}
