using ManageLife.Base;

namespace ManageLife.Helpers
{
	public class MenuRegister : IMenuRegister
	{
		public List<MenuItem> GetListMenuItems()
		{
			var listMenuItems = new List<MenuItem>();

			listMenuItems.AddRange(new List<MenuItem>
			{
				new MenuItem("Dashboard", "/Dashboard", "fa-solid fa-house"),
				new MenuItem("Users", "/Users", "fa-solid fa-users", new List<MenuItem>())
			});

			return listMenuItems;
		}
	}
}
