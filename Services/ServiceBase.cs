using AutoMapper;
using ManageLife.Contexts;
using ManageLife.Core;

namespace ManageLife.Services
{
    public abstract class ServiceBase<T>
    {
        protected readonly IAppLogger<T> _logger;
        protected readonly IUserContext _userContext;
        protected readonly IMapper _mapper;

        protected ServiceBase(IAppLogger<T> logger, IUserContext userContext, IMapper mapper)
        {
            _logger = logger;
            _userContext = userContext;
            _mapper = mapper;
        }
    }
}
