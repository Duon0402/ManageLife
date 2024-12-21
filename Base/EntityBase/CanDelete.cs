namespace ManageLife.Base
{
    public interface CanDelete
    {
        public bool IsDeleted { get; set; }
        public string DeletedUser { get; set; }
        public DateTime DeletedTime { get; set; }
    }
}