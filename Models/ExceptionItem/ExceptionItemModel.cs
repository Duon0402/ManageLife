namespace ManageLife.Models
{
    public class ExceptionItemModel
    {
        public string Id { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string? Description { get; set; }
    }
}
