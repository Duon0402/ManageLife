namespace ManageLife.Base
{
    public interface CanDelete
    {
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}