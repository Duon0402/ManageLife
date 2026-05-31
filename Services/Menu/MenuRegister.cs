using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Controllers.Admin;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class MenuRegister : IMenuRegister
    {
        private readonly ICacheService _cache;
        private readonly IPermissionService _permissionService;
        private readonly IUserContext _userContext;

        public MenuRegister(ICacheService cache, IPermissionService permissionService, IUserContext userContext)
        {
            _cache = cache;
            _permissionService = permissionService;
            _userContext = userContext;
        }

        public async Task<List<MenuItem>> GetListMenuItemsAsync()
        {
            var cacheItem = CacheSettings.MenuItems();
            var baseMenu = await _cache.TryGetValueAsync<List<MenuItem>>(cacheItem);
            if (baseMenu == null)
            {
                baseMenu = BuildMenuItems();
                await _cache.SetAsync(baseMenu, cacheItem);
            }

            return await FilterMenuByPermissionAsync(baseMenu);
        }

        private List<MenuItem> BuildMenuItems()
        {
            return new List<MenuItem>
            {
                new MenuItem<DashboardController>("Dashboard", x => x.Index(), "fa-solid fa-house"),

                new MenuItem<CronJobController>("Cron Job", x => x.Index(), "fa-solid fa-calendar-days"),

                new MenuItem<DatabaseController>("Database", x => x.Index(), "fa-solid fa-database"),

                new MenuItem("Language", "fa-solid fa-language", new List<MenuItem>
                {
                    new MenuItem<LanguageController>("Language", x => x.Index(), "fa-regular fa-circle fa-2xs"),
                    new MenuItem<TranslationController>("Translation", x => x.Index(default(CancellationToken)), "fa-regular fa-circle fa-2xs"),
                }),
                new MenuItem("User Management", "fa-solid fa-user",new List<MenuItem>
                {
                    new MenuItem<UserController>("User", x => x.Index(), "fa-solid fa-user"),
                    new MenuItem<RoleController>("Role", x => x.Index(), "fa-solid fa-shield"),
                    new MenuItem<UserTelegramConnectionController>("Telegram Connection", x => x.Index(), "fa-solid fa-arrow-right-arrow-left"),
                }),

                new MenuItem("Telegram Bot", "fa-brands fa-telegram", new List<MenuItem>
                {
                    new MenuItem<TelegramBotCommandController>("Bot Commands", x => x.Index(), "fa-solid fa-terminal"),
                })
            };
        }

        private async Task<List<MenuItem>> FilterMenuByPermissionAsync(List<MenuItem> allMenuItems)
        {
            var userId = _userContext.GetUserId();

            HashSet<string> userPermissions = new(StringComparer.OrdinalIgnoreCase);

            if (userId.IsNotEmpty())
            {
                var req = new GetAssignedPermissionsByUserIdRequest { UserId = userId };
                var res = await _permissionService.GetAssignedPermissionsByUserIdAsync(req);

                if (res.IsOk() && res.Data.IsNotEmpty())
                {
                    userPermissions = res.Data
                        .Select(x => x.Code)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                }
            }

            // Build new instances to avoid mutating the shared cached list
            List<MenuItem> FilterRecursive(List<MenuItem> items)
            {
                var result = new List<MenuItem>();

                foreach (var item in items)
                {
                    bool hasPermission =
                        item.PermissionCode.IsEmpty() ||
                        userPermissions.Contains(item.PermissionCode);

                    if (item.HasSubItems)
                    {
                        var filteredSub = FilterRecursive(item.SubItems);
                        if (filteredSub.Count > 0)
                        {
                            result.Add(item.WithSubItems(filteredSub));
                        }
                    }
                    else if (hasPermission)
                    {
                        result.Add(item);
                    }
                }

                return result;
            }

            return FilterRecursive(allMenuItems);
        }
    }
}
