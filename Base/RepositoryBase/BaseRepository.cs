using Microsoft.EntityFrameworkCore;

namespace ManageLife.Base
{
    public class BaseRepository
    {
        protected readonly DbContext _context;

        public BaseRepository(DbContext context)
        {
            _context = context;
        }
    }
}