using ManageLife.Core;
using ManageLife.Models;
using Telegram.Bot.Types;

namespace ManageLife.Interfaces
{
    public interface ITelegramService
    {
        Task<Result> SendMessageAsync(SendTelegramMessageRequest request, CancellationToken ct = default);
        Task<Result> SendMessageToChatAsync(long chatId, string message, CancellationToken ct = default);

        Task HandleUpdateAsync(Update update, CancellationToken ct = default);

        /// <summary>Header X-Telegram-Bot-Api-Secret-Token khớp WebhookSecret đã cấu hình (chưa cấu hình → false).</summary>
        bool IsValidWebhookSecret(string? secretToken);

        /// <summary>Tạo mã liên kết Telegram (dùng 1 lần, 5 phút) cho người dùng hiện tại.</summary>
        Task<Result<TelegramLinkModel>> CreateLinkAsync(CancellationToken ct = default);

        /// <summary>Gỡ liên kết Telegram của người dùng hiện tại.</summary>
        Task<Result> UnlinkAsync(CancellationToken ct = default);

        Task<Result<string>> RegisterWebhookAsync(string url, CancellationToken ct = default);

        Task<Result<object>> GetWebhookStatusAsync(CancellationToken ct = default);

        Task<Result> SetDefaultCommandsAsync(CancellationToken ct = default);

        Task<Result<List<BotCommand>>> GetListTelegramBotCommands(CancellationToken ct = default);
    }
}
