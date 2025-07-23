using ManageLife.Base;
using Telegram.Bot;

namespace ManageLife.Services
{
    public class TelegramService
    {
        private readonly IConfiguration _config;
        private readonly string? _chatId;
        private readonly TelegramBotClient _botClient;

        public TelegramService(IConfiguration config)
        {
            _config = config;
            var botToken = _config["TelegramSettings:BotToken"] ?? "";
            _chatId = _config["TelegramSettings:ChatId"];
            _botClient = new TelegramBotClient(botToken);
        }

        public async Task<Result> SendMessageAsync(string message)
        {
            string msg;
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    msg = "Vui lòng nhập tin nhắn cần gửi";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                if (_chatId == null)
                {
                    msg = "Không lấy được ChatId";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                await _botClient.SendMessage(_chatId, message);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi gửi tin nhắn";
                return Result.Exception(msg, ex);
            }
        }

    }
}
