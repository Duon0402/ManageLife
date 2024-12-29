namespace ManageLife.Base
{
    public interface CanUpdate
    {
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}