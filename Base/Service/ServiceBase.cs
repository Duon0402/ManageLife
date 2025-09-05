using ManageLife.Data;

namespace ManageLife.Base
{
    public class ServiceBase
    {
        protected readonly AppDbContext _context;

        public ServiceBase(AppDbContext context)
        {
            _context = context;
        }
    }
}
