using ManageLife.Core;
using ManageLife.Models;
using Telegram.Bot.Types;

namespace ManageLife.Interfaces
{
    public interface ITelegramService
    {
        Task<Result> SendMessageAsync(SendTelegramMessageRequest request, CancellationToken ct = default);

        Task HandleUpdateAsync(Update update, CancellationToken ct = default);

        Task<Result<string>> RegisterWebhookAsync(string url, CancellationToken ct = default);

        Task<Result<object>> GetWebhookStatusAsync(CancellationToken ct = default);

        Task<Result> SetDefaultCommandsAsync(CancellationToken ct = default);

        Task<Result<List<BotCommand>>> GetListTelegramBotCommands(CancellationToken ct = default);
    }
}
