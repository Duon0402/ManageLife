namespace ManageLife.Base
{
	public interface ISoftDelete
	{
		public string? DeletedUser { get; set; }
		public DateTime? DeletedTime { get; set; }
		public bool IsDeleted { get; set; }
	}
}