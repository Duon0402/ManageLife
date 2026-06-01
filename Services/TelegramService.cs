using ManageLife.Commons;
using ManageLife.Core;
using ManageLife.Contexts;
using ManageLife.Entities;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Settings;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ManageLife.Services
{
    public class TelegramService : ServiceBase<TelegramService>, ITelegramService
    {
        private readonly string? _chatId;
        private readonly TelegramBotClient _botClient;
        private readonly ISettingService _settingService;
        private readonly ITelegramBotCommandService _botCommandService;
        private readonly IUserRepository _userRepo;
        private readonly IUserTelegramConnectionRepository _connectionRepo;
        private readonly ICacheService _cache;

        public TelegramService(
            IOptions<TelegramSettings> options,
            ISettingService settingService,
            TelegramBotClient botClient,
            ITelegramBotCommandService botCommandService,
            IUserRepository userRepo,
            IUserTelegramConnectionRepository connectionRepo,
            ICacheService cache,
            IAppLogger<TelegramService> logger,
            IUserContext userContext) : base(logger, userContext)
        {
            _settingService = settingService;
            _botClient = botClient;
            _botCommandService = botCommandService;
            _userRepo = userRepo;
            _connectionRepo = connectionRepo;
            _cache = cache;
            _chatId = options.Value.ChatId;
        }

        public async Task<Result> SendMessageAsync(SendTelegramMessageRequest request, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

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

        public async Task<Result> SendMessageToChatAsync(long chatId, string message, CancellationToken ct = default)
        {
            try
            {
                await _botClient.SendMessage(chatId, message, cancellationToken: ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Đã có lỗi xảy ra khi gửi tin nhắn Telegram";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task HandleUpdateAsync(Update update, CancellationToken ct = default)
        {
            try
            {
                if (update.Message is not { } message) return;
                if (message.Text is not { } messageText) return;

                var chatId = message.Chat.Id;
                var messageId = message.MessageId;
                var isPrivate = message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private;

                _logger.Info("Received '{messageText}' in chat {chatId}", messageText, chatId);

                if (messageText.StartsWith("/"))
                {
                    await HandleCommandAsync(chatId, messageText, isPrivate, ct);
                }
                else if (isPrivate)
                {
                    // Conversation flow chỉ hoạt động trong private chat
                    await HandleConversationAsync(chatId, messageId, messageText, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Đã có lỗi xảy ra khi xử lý cập nhật từ Telegram");
            }
        }

        // ──────────────────── Commands ────────────────────

        private async Task HandleCommandAsync(long chatId, string messageText, bool isPrivate, CancellationToken ct)
        {
            var rawCommand = messageText.Split(' ')[0].ToLower();
            // Trong group chat, command có dạng /link@botname — cần strip phần @botname
            var atIndex = rawCommand.IndexOf('@');
            var command = atIndex > 0 ? rawCommand[..atIndex] : rawCommand;

            switch (command)
            {
                case "/start":
                    await _botClient.SendMessage(chatId,
                        "Chào mừng bạn đến với *ManageLife Bot*\\!\nGửi /help để xem các lệnh hỗ trợ\\.",
                        parseMode: ParseMode.MarkdownV2, cancellationToken: ct);
                    break;

                case "/info":
                    await _botClient.SendMessage(chatId,
                        $"Chat ID của bạn là: `{chatId}`",
                        parseMode: ParseMode.MarkdownV2, cancellationToken: ct);
                    break;

                case "/help":
                    await _botClient.SendMessage(chatId,
                        "📋 *Các lệnh hỗ trợ:*\n\n" +
                        "/start \\- Bắt đầu\n" +
                        "/info \\- Lấy Chat ID của bạn\n" +
                        "/link \\- Liên kết tài khoản ManageLife\n" +
                        "/help \\- Hướng dẫn",
                        parseMode: ParseMode.MarkdownV2, cancellationToken: ct);
                    break;

                case "/link":
                    if (isPrivate)
                    {
                        await StartLinkFlowAsync(chatId, ct);
                    }
                    else
                    {
                        // Group chat: bot không nhận tin nhắn thường (privacy mode)
                        // → dùng format 1 lần hoặc nhắn riêng với bot
                        var parts = messageText.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 3)
                            await HandleLinkWithCredentialsAsync(chatId, parts[1], parts[2], ct);
                        else
                            await _botClient.SendMessage(chatId,
                                "Trong nhóm, hãy dùng lệnh: `/link username password`\n" +
                                "Hoặc nhắn riêng với bot để bảo mật hơn.",
                                parseMode: ParseMode.Markdown, cancellationToken: ct);
                    }
                    break;

                default:
                    await _botClient.SendMessage(chatId,
                        "Lệnh không hợp lệ. Gửi /help để xem danh sách lệnh.",
                        cancellationToken: ct);
                    break;
            }
        }

        // ──────────────────── Conversational flow ────────────────────

        private async Task StartLinkFlowAsync(long chatId, CancellationToken ct)
        {
            var state = new TelegramLinkState { Step = TelegramLinkStep.WaitingUsername };
            await _cache.SetAsync(state, CacheSettings.TelegramLinkState(chatId));

            await _botClient.SendMessage(chatId,
                "🔗 *Liên kết tài khoản ManageLife*\n\nNhập *username* của bạn:",
                parseMode: ParseMode.Markdown,
                replyMarkup: new ForceReplyMarkup(),
                cancellationToken: ct);
        }

        private async Task HandleConversationAsync(long chatId, int messageId, string text, CancellationToken ct)
        {
            var cacheItem = CacheSettings.TelegramLinkState(chatId);
            var state = await _cache.TryGetValueAsync<TelegramLinkState>(cacheItem);

            if (state == null) return;

            switch (state.Step)
            {
                case TelegramLinkStep.WaitingUsername:
                    state.Step = TelegramLinkStep.WaitingPassword;
                    state.Username = text.Trim();
                    await _cache.SetAsync(state, cacheItem);

                    await _botClient.SendMessage(chatId,
                        $"Username: *{state.Username}*\n\nNhập *password* của bạn:",
                        parseMode: ParseMode.Markdown,
                        replyMarkup: new ForceReplyMarkup(),
                        cancellationToken: ct);
                    break;

                case TelegramLinkStep.WaitingPassword:
                    await _cache.RemoveAsync(cacheItem);

                    // Xóa tin nhắn chứa password để bảo mật
                    try { await _botClient.DeleteMessage(chatId, messageId, ct); } catch { }

                    await HandleLinkWithCredentialsAsync(chatId, state.Username!, text.Trim(), ct);
                    break;
            }
        }

        private async Task HandleLinkWithCredentialsAsync(long chatId, string username, string password, CancellationToken ct)
        {
            var user = await _userRepo.FirstOrDefaultAsync(x => x.UserName == username && !x.IsDeleted && x.IsActive, ct);
            if (user == null)
            {
                await _botClient.SendMessage(chatId, "❌ Tên đăng nhập hoặc mật khẩu không đúng.", cancellationToken: ct);
                return;
            }

            bool passwordValid = PasswordHelper.IsLegacyHash(user.HashPassword)
                ? PasswordHelper.VerifyLegacy(password, user.HashPassword)
                : PasswordHelper.Verify(password, user.HashPassword);

            if (!passwordValid)
            {
                await _botClient.SendMessage(chatId, "❌ Tên đăng nhập hoặc mật khẩu không đúng.", cancellationToken: ct);
                return;
            }

            var existing = await _connectionRepo.FirstOrDefaultAsync(x => x.UserId == user.Id && !x.IsDeleted, ct);
            if (existing != null)
            {
                existing.ChatId = chatId;
                await _connectionRepo.UpdateAsync(existing, ct);
                await _botClient.SendMessage(chatId,
                    $"✅ Đã cập nhật liên kết tài khoản *{username}* với Telegram này.",
                    parseMode: ParseMode.Markdown, cancellationToken: ct);
            }
            else
            {
                var entity = new UserTelegramConnectionEntity
                {
                    Id = IdHelper.NewId(),
                    UserId = user.Id,
                    ChatId = chatId,
                    CreatedUser = username
                };
                await _connectionRepo.InsertAsync(entity, ct);
                await _botClient.SendMessage(chatId,
                    $"✅ Đã liên kết tài khoản *{username}* thành công!",
                    parseMode: ParseMode.Markdown, cancellationToken: ct);
            }
        }

        // ──────────────────── Webhook & Commands ────────────────────

        public async Task<Result<string>> RegisterWebhookAsync(string url, CancellationToken ct = default)
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

        public async Task<Result<object>> GetWebhookStatusAsync(CancellationToken ct = default)
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

        public async Task<Result> SetDefaultCommandsAsync(CancellationToken ct = default)
        {
            try
            {
                var dbResult = await _botCommandService.GetListAsync(ct);
                if (!dbResult.IsOk() || dbResult.Data == null || dbResult.Data.Count == 0)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Không có command nào trong hệ thống. Hãy thêm commands trước khi đồng bộ.");

                var commands = dbResult.Data.Select(x => new BotCommand
                {
                    Command = x.Command,
                    Description = x.Description
                }).ToList();

                await _botClient.SetMyCommands(commands, cancellationToken: ct);
                return Result.Ok($"Đã đồng bộ {commands.Count} command lên Telegram");
            }
            catch (Exception ex)
            {
                return Result.Exception("Lỗi khi đồng bộ commands lên Telegram", ex);
            }
        }

        public async Task<Result<List<BotCommand>>> GetListTelegramBotCommands(CancellationToken ct = default)
        {
            try
            {
                var commands = await _botClient.GetMyCommands();
                return Result.Ok(commands.ToList());
            }
            catch (Exception ex)
            {
                var msg = "Đã có lỗi xảy ra khi lấy danh sách commands";
                _logger.Error(ex, msg);
                return Result.Exception<List<BotCommand>>(msg, ex);
            }
        }
    }
}
