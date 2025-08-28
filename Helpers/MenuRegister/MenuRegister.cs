using ManageLife.Base;
using ManageLife.Controllers.Admin;

namespace ManageLife.Helpers
{
    public class MenuRegister : IMenuRegister
    {
        public List<MenuItem> GetListMenuItems()
        {
            var listMenuItems = new List<MenuItem>
            {
                new MenuItem<DashboardController>("Dashboard", x => x.Index(), "fa-solid fa-house"),
                new MenuItem<CronJobController>("Cron Job", x => x.Index(), "fa-solid fa-calendar-days"),
                new MenuItem<LanguageController>("Language", x => x.Index(), "fa-solid fa-language"),
                new MenuItem<TranslationController>("Translation", x => x.Index(), "fa-solid fa-language"),
            };

            return listMenuItems;
        }
    }
}
