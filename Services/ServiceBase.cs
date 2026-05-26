using AutoMapper;
using ManageLife.Contexts;

namespace ManageLife.Services
{
    public abstract class ServiceBase
    {
        protected readonly IMapper _mapper;
        protected readonly IUserContext _userContext;

        protected ServiceBase(IMapper mapper, IUserContext userContext)
        {
            _mapper = mapper;
            _userContext = userContext;
        }
    }
}
