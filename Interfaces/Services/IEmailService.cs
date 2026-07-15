using ManageLife.Core;

namespace ManageLife.Interfaces
{
    public interface IEmailService
    {
        Task<Result> SendAsync(string to, string subject, string body, bool isHtml = true, CancellationToken ct = default);
    }
}
