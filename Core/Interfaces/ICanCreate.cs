namespace ManageLife.Core
{
	public interface ICanCreate
	{
		public string CreatedUser { get; set; }
		public DateTime CreatedTime { get; set; }
	}
}