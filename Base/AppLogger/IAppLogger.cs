namespace ManageLife.Base
{
    public interface IAppLogger<T>
    {
        void Debug(string message, object? data = null);
        void Info(string message, object? data = null);
        void Warning(string message, object? data = null);
        void Error(Exception ex, string message, object? data = null);
    }
}
