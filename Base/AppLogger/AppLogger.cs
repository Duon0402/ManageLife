namespace ManageLife.Base
{
    public class AppLogger<T> : IAppLogger<T>
    {
        private readonly ILogger<T> _logger;

        public AppLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void Debug(string message, object? data = null)
        {
            _logger.LogDebug("{Message} {@Data}", message, data);
        }

        public void Error(Exception ex, string message, object? data = null)
        {
            _logger.LogError(ex, "{Message} {@Data}", message, data);
        }

        public void Info(string message, object? data = null)
        {
            _logger.LogInformation("{Message} {@Data}", message, data);
        }

        public void Warning(string message, object? data = null)
        {
            _logger.LogWarning("{Message} {@Data}", message, data);
        }
    }
}
