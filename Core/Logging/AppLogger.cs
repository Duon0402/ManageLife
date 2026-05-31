using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ManageLife.Core
{
    public class AppLogger<T> : IAppLogger<T>
    {
        private readonly ILogger<T> _logger;
        private static readonly string _className = typeof(T).Name;

        public AppLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void Debug(string message, params object[] args)
            => _logger.LogDebug(message, args);

        public void Info(string message, params object[] args)
            => _logger.LogInformation(message, args);

        public void Warning(string message, params object[] args)
            => _logger.LogWarning(message, args);

        public void Error(Exception ex, string message, params object[] args)
        {
            using (_logger.BeginScope("{ClassName}", _className))
                _logger.LogError(ex, message, args);
        }

        public void Fatal(Exception ex, string message, params object[] args)
        {
            using (_logger.BeginScope("{ClassName}", _className))
                _logger.LogCritical(ex, message, args);
        }

        public IDisposable BeginOperation(string operationName, [CallerMemberName] string caller = "")
            => new OperationScope(_logger, _className, caller, operationName);

        private sealed class OperationScope : IDisposable
        {
            private readonly ILogger _logger;
            private readonly string _className;
            private readonly string _caller;
            private readonly string _operationName;
            private readonly Stopwatch _sw;

            public OperationScope(ILogger logger, string className, string caller, string operationName)
            {
                _logger = logger;
                _className = className;
                _caller = caller;
                _operationName = operationName;
                _sw = Stopwatch.StartNew();
                _logger.LogDebug("[{Class}.{Method}] {Operation} started", _className, _caller, _operationName);
            }

            public void Dispose()
            {
                _sw.Stop();
                _logger.LogDebug("[{Class}.{Method}] {Operation} completed in {ElapsedMs}ms",
                    _className, _caller, _operationName, _sw.ElapsedMilliseconds);
            }
        }
    }
}
