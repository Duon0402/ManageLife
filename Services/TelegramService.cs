using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Extensions;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using Telegram.Bot;

namespace ManageLife.Services
{
    public class TelegramService : ServiceBase, ITelegramService
    {
        private readonly IConfiguration _config;
        private readonly string? _chatId;
        private readonly TelegramBotClient _botClient;

        public TelegramService(AppDbContext context, IConfiguration config) : base(context)
        {
            _config = config;
            var botToken = _config["TelegramSettings:BotToken"] ?? "";
            _chatId = _config["TelegramSettings:ChatId"];
            _botClient = new TelegramBotClient(botToken);
        }

        public async Task<Result> SendMessageAsync(SendMessageRequest request)
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
                    msg = await TranslationHelper.TAsync(TranslationKey.Common.Message.DataInvalid);
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

    }
}
