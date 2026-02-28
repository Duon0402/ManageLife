using ManageLife.Base;
using ManageLife.Models;
using Telegram.Bot.Types;

namespace ManageLife.Interfaces
{
    public interface ITelegramService
    {
        Task<Result> SendMessageAsync(SendTelegramMessageRequest request);

        Task HandleUpdateAsync(Update update);

        Task<Result<string>> RegisterWebhookAsync(string url);

        Task<Result<object>> GetWebhookStatusAsync();

        Task<Result> SetDefaultCommandsAsync();

        Task<Result<List<BotCommand>>> GetListTelegramBotCommands();
    }
}
