using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITelegramService
    {
        Task<Result> SendMessageAsync(SendMessageRequest request);
    }
}
