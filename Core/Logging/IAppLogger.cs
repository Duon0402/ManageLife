using System.Runtime.CompilerServices;

namespace ManageLife.Core
{
    public interface IAppLogger<T>
    {
        void Debug(string message, params object[] args);
        void Info(string message, params object[] args);
        void Warning(string message, params object[] args);
        void Error(Exception ex, string message, params object[] args);
        void Fatal(Exception ex, string message, params object[] args);
        IDisposable BeginOperation(string operationName, [CallerMemberName] string caller = "");
    }
}
