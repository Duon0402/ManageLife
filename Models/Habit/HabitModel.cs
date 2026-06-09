namespace ManageLife.Models
{
    public class HabitModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
