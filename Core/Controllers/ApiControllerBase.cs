using ManageLife.Data;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiControllerBase : ControllerBase
    {
        public ApiControllerBase()
        {
        }
    }
}