using ManageLife.Data;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Base
{
    public class WebControllerBase : Controller
    {
        private readonly AppDbContext _context;

        public WebControllerBase(AppDbContext context)
        {
            _context = context;
        }
    }
}
