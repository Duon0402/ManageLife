using ManageLife.Data;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Base
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiControllerBase : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger? _logger;

        public ApiControllerBase(AppDbContext context, ILogger? logger = null)
        {
            _context = context;
            _logger = logger;
        }
    }
}