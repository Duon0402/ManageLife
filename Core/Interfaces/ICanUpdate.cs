namespace ManageLife.Core
{
	public interface ICanUpdate
	{
		public string? UpdatedUser { get; set; }
		public DateTime? UpdatedTime { get; set; }
	}
}