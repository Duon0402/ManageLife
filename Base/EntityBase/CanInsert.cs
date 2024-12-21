namespace ManageLife.Base
{
    public interface CanCreate
    {
        public DateTime CreatedTime { get; set; }
        public string CreatedUser { get; set; }
    }
}