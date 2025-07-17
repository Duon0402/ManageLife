namespace ManageLife.Base
{
    public interface ICanUpdate
    {
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}