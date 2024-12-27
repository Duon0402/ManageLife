using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Base
{
    public class WebControllerBase : Controller
    {
        private readonly ILogger _logger;

        public WebControllerBase(ILogger logger)
        {
            _logger = logger;
        }
    }
}
