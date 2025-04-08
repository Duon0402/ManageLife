namespace ManageLife.Base
{
    public interface SoftDelete
    {
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}