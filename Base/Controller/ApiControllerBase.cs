using ManageLife.Data;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Base
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiControllerBase : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApiControllerBase(AppDbContext context)
        {
            _context = context;
        }
    }
}