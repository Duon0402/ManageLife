namespace ManageLife.Base
{
	public static class DateTimeHelper
	{
		public static DateTime Now()
		{
			return DateTime.Now;
		}

		public static DateTime UtcNow()
		{
			return DateTime.UtcNow;
		}
	}
}