using ManageLife.Core;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ManageLife.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly IConfiguration _config;
        private readonly string? _chatId;
        private readonly TelegramBotClient _botClient;
        private readonly IAppLogger<TelegramService> _logger;
        private readonly ISettingService _settingService;

        public TelegramService(IConfiguration config, IAppLogger<TelegramService> logger, ISettingService settingService)
        {
            _config = config;
            _logger = logger;
            _settingService = settingService;
            var botToken = _config["TelegramSettings:BotToken"] ?? "";
            _chatId = _config["TelegramSettings:ChatId"];
            _botClient = new TelegramBotClient(botToken);
        }

        public async Task<Result> SendMessageAsync(SendTelegramMessageRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                if (_chatId == null)
                {
                    msg = "Không tìm thấy ChatId được cấu hình";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                await _botClient.SendMessage(_chatId, request.Message);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi gửi tin nhắn";
                return Result.Exception(msg, ex);
            }
        }

        public async Task HandleUpdateAsync(Update update)
        {
            string msg;
            try
            {
                if (update.Message is not { } message)
                    return;

                if (message.Text is not { } messageText)
                    return;

                var chatId = message.Chat.Id;

                msg = "Received a '{messageText}' message in chat {chatId}.";
                _logger.Info(msg, messageText, chatId);

                if (messageText.StartsWith("/"))
                {
                    await HandleCommandAsync(chatId, messageText);
                }
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi xử lý cập nhật từ Telegram";
                _logger.Error(ex, msg);
            }
        }

        private async Task HandleCommandAsync(long chatId, string messageText)
        {
            var command = messageText.Split(' ')[0].ToLower();

            switch (command)
            {
                case "/start":
                    await _botClient.SendMessage(chatId, "Chào mừng bạn đến với ManageLife Bot! Hãy gửi /help để xem các lệnh hỗ trợ.");
                    break;
                case "/info":
                    await _botClient.SendMessage(chatId, $"Chat ID của bạn là: {chatId}");
                    break;
                case "/help":
                    await _botClient.SendMessage(chatId, "Các lệnh hỗ trợ:\n/start - Bắt đầu\n/info - Lấy thông tin cá nhân\n/help - Hướng dẫn");
                    break;
                default:
                    await _botClient.SendMessage(chatId, "Lệnh không hợp lệ. Gửi /help để xem danh sách lệnh.");
                    break;
            }
        }

        public async Task<Result<string>> RegisterWebhookAsync(string url)
        {
            string msg;
            try
            {
                if (url.IsEmpty())
                {
                    msg = "Webhook URL không được để trống";
                    return Result.Error<string>(Result.DATA_INVALID.Code, msg);
                }

                await _botClient.SetWebhook(url);
                _logger.Info("Telegram Webhook registered successfully to {url}", url);
                return Result.Ok("Webhook registered successfully");
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi đăng ký Webhook";
                _logger.Error(ex, msg);
                return Result.Exception<string>(msg, ex);
            }
        }

        public async Task<Result<object>> GetWebhookStatusAsync()
        {
            try
            {
                var info = await _botClient.GetWebhookInfo();
                return Result.Ok<object>(info);
            }
            catch (Exception ex)
            {
                return Result.Exception<object>("Lỗi khi lấy thông tin Webhook", ex);
            }
        }

        public async Task<Result> SetDefaultCommandsAsync()
        {
            try
            {
                var commands = new List<BotCommand>
                {
                    new() { Command = "start", Description = "Bắt đầu sử dụng bot" },
                    new() { Command = "info", Description = "Lấy thông tin cá nhân của bạn" },
                    new() { Command = "help", Description = "Xem hướng dẫn sử dụng" }
                };

                await _botClient.SetMyCommands(commands);
                return Result.Ok("Registered commands successfully");
            }
            catch (Exception ex)
            {
                return Result.Exception("Lỗi khi đăng ký commands", ex);
            }
        }

        public async Task<Result<List<BotCommand>>> GetListTelegramBotCommands()
        {
            string msg;
            try
            {
                var commands = await _botClient.GetMyCommands();
                return Result.Ok(commands.ToList());
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi lấy danh sách commands";
                _logger.Error(ex, msg);
                return Result.Exception<List<BotCommand>>(msg, ex);
            }
        }
    }
}
