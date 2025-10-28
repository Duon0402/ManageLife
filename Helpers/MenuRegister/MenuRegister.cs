using ManageLife.Base;
using ManageLife.Controllers.Admin;

namespace ManageLife.Helpers
{
    // TODO: Thêm phần kiêm trả để ẩn menu Item nếu yêu cầu permission
    public class MenuRegister : IMenuRegister
    {
        public List<MenuItem> GetListMenuItems()
        {
            var listMenuItems = new List<MenuItem>
            {
                new MenuItem<DashboardController>("Dashboard", x => x.Index(), "fa-solid fa-house"),
                new MenuItem<CronJobController>("Cron Job", x => x.Index(), "fa-solid fa-calendar-days"),
                new MenuItem("Language", null, "fa-solid fa-language", new List<MenuItem>
                {
                    new MenuItem<LanguageController>("Language", x => x.Index(), "fa-regular fa-circle fa-2xs"),
                    new MenuItem<TranslationController>("Translation", x => x.Index(), "fa-regular fa-circle fa-2xs"),
                }),

            };

            return listMenuItems;
        }
    }
}
