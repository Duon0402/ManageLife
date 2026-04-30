namespace ManageLife.Core
{
	public static class IdHeper
	{
		public static string NewId()
		{
			return Guid.NewGuid().ToString();
		}
	}
}
