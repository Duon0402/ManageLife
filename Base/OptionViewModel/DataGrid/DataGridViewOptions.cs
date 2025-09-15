namespace ManageLife.Base
{
	public class DataGridViewOptions
	{
		public bool AllowInsert { get; set; } = false;
		public bool AllowUpdate { get; set; } = false;
		public bool AllowDelete { get; set; } = false;
		public string? Id { get; set; }
	}
}