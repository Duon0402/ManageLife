using ManageLife.Base;

namespace ManageLife.Helpers
{
    public class MenuRegister : IMenuRegister
    {
        public List<MenuItem> GetListMenuItems()
        {
            var listMenuItems = new List<MenuItem>
            {
                new MenuItem("Dashboard", "/Dashboard", "fa-solid fa-house"),
                new MenuItem("Users", icon: "fa-solid fa-users", subItems: new List<MenuItem>
                {
                    new MenuItem("User List", "/Users", "fa-solid fa-list"),
                    new MenuItem("Create User", "/Users/Create", "fa-solid fa-plus")
                })
            };

            return listMenuItems;
        }
    }
}
