using ManageLife.Contexts;
using ManageLife.Core;

namespace ManageLife.Services
{
    public abstract class ServiceBase<T>
    {
        protected readonly IAppLogger<T> _logger;
        protected readonly IUserContext _userContext;

        protected ServiceBase(IAppLogger<T> logger, IUserContext userContext)
        {
            _logger = logger;
            _userContext = userContext;
        }
    }
}
