using ManageLife.Commons;
using ManageLife.Core;
using ManageLife.Contexts;
using ManageLife.Entities;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
        private readonly string? _webhookSecret;

        private static readonly TimeSpan LinkCodeLifetime = TimeSpan.FromMinutes(5);
        //NOTE: Mã do CreateLinkAsync sinh: 24 ký tự base64url
        private static readonly Regex LinkCodePattern = new("^[A-Za-z0-9_-]{24}$", RegexOptions.Compiled);
        //NOTE: Telegram chỉ nhận secret_token 1–256 ký tự A-Z a-z 0-9 _ -
        private static readonly Regex WebhookSecretPattern = new("^[A-Za-z0-9_-]{1,256}$", RegexOptions.Compiled);

        public TelegramService(
            IOptions<TelegramOptions> options,
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
            _webhookSecret = options.Value.WebhookSecret;
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
                var isPrivate = message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private;

                //NOTE: Không log nội dung tin nhắn (có thể chứa thông tin nhạy cảm), chỉ log chat
                _logger.Debug("Telegram: nhận tin nhắn trong chat {chatId}", chatId);

                if (messageText.StartsWith("/"))
                    await HandleCommandAsync(chatId, messageText, isPrivate, ct);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Đã có lỗi xảy ra khi xử lý cập nhật từ Telegram");
            }
        }

        // ──────────────────── Commands ────────────────────

        private const string LinkGuide = "Để liên kết, mở app ManageLife → Cài đặt → Tài khoản → Telegram → Liên kết.";

        private async Task HandleCommandAsync(long chatId, string messageText, bool isPrivate, CancellationToken ct)
        {
            var parts = messageText.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var rawCommand = parts[0].ToLowerInvariant();
            // Trong group chat, command có dạng /start@botname — cần strip phần @botname
            var atIndex = rawCommand.IndexOf('@');
            var command = atIndex > 0 ? rawCommand[..atIndex] : rawCommand;

            switch (command)
            {
                case "/start" when parts.Length >= 2:
                    // Mở từ deep link t.me/<bot>?start=<mã> trong app
                    await LinkByCodeAsync(chatId, isPrivate, parts[1], ct);
                    break;

                case "/start":
                    await _botClient.SendMessage(chatId,
                        "Chào mừng bạn đến với ManageLife Bot!\n" + LinkGuide + "\nGửi /help để xem các lệnh hỗ trợ.",
                        cancellationToken: ct);
                    break;

                case "/info":
                    await _botClient.SendMessage(chatId,
                        $"Chat ID của bạn là: `{chatId}`",
                        parseMode: ParseMode.MarkdownV2, cancellationToken: ct);
                    break;

                case "/help":
                    await _botClient.SendMessage(chatId,
                        "📋 Các lệnh hỗ trợ:\n\n" +
                        "/start - Bắt đầu\n" +
                        "/info - Lấy Chat ID của bạn\n" +
                        "/link - Hướng dẫn liên kết tài khoản ManageLife\n" +
                        "/unlink - Gỡ liên kết Telegram này khỏi tài khoản\n" +
                        "/help - Hướng dẫn",
                        cancellationToken: ct);
                    break;

                case "/link":
                    //NOTE: Không còn liên kết bằng username/mật khẩu qua chat; chỉ liên kết bằng mã từ app
                    await _botClient.SendMessage(chatId, LinkGuide, cancellationToken: ct);
                    break;

                case "/unlink":
                    await UnlinkChatAsync(chatId, isPrivate, ct);
                    break;

                default:
                    await _botClient.SendMessage(chatId,
                        "Lệnh không hợp lệ. Gửi /help để xem danh sách lệnh.",
                        cancellationToken: ct);
                    break;
            }
        }

        // ──────────────────── Liên kết bằng mã ────────────────────

        public async Task<Result<TelegramLinkModel>> CreateLinkAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<TelegramLinkModel>(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var botUsername = await GetBotUsernameAsync(ct);
                if (botUsername.IsEmpty()) return Result.Error<TelegramLinkModel>(Result.DATA_NOT_EXISTED.Code, "Không lấy được thông tin bot Telegram");

                //NOTE: 18 byte ngẫu nhiên → 24 ký tự base64url, hợp lệ cho tham số start của Telegram (A-Z a-z 0-9 _ -)
                var code = Convert.ToBase64String(RandomNumberGenerator.GetBytes(18)).Replace('+', '-').Replace('/', '_');
                var cacheItem = CacheSettings.TelegramLinkCode(code);
                await _cache.SetAsync(userId, cacheItem);
                //NOTE: CacheService nuốt lỗi khi ghi: đọc lại để không trả về liên kết chết khi Redis lỗi
                if (await _cache.TryGetValueAsync<string>(cacheItem) != userId)
                    return Result.Error<TelegramLinkModel>(Result.DATA_NOT_CREATE.Code, "Không tạo được mã liên kết, thử lại sau");

                return Result.Ok(new TelegramLinkModel
                {
                    DeepLink = $"https://t.me/{botUsername}?start={code}",
                    ExpiresAt = DateTime.SpecifyKind(DateTimeHelper.UtcNow().Add(LinkCodeLifetime), DateTimeKind.Utc)
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi tạo mã liên kết Telegram");
                return Result.Exception<TelegramLinkModel>("Có lỗi xảy ra khi tạo liên kết Telegram", ex);
            }
        }

        public async Task<Result> UnlinkAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var connections = await _connectionRepo.Query().Where(x => x.UserId == userId && !x.IsDeleted).ToListAsync(ct);
                foreach (var connection in connections)
                    await _connectionRepo.DeleteAsync(connection, ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi gỡ liên kết Telegram");
                return Result.Exception("Có lỗi xảy ra khi gỡ liên kết Telegram", ex);
            }
        }

        public bool IsValidWebhookSecret(string? secretToken)
        {
            //NOTE: Chưa cấu hình secret thì chặn hết, tránh mở webhook cho bất kỳ ai giả mạo update
            if (_webhookSecret.IsEmpty() || secretToken.IsEmpty()) return false;
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(secretToken!), Encoding.UTF8.GetBytes(_webhookSecret!));
        }

        /// <summary>Chủ chat luôn tự gỡ được chat của mình (vd lỡ bấm liên kết của người khác).</summary>
        private async Task UnlinkChatAsync(long chatId, bool isPrivate, CancellationToken ct)
        {
            if (!isPrivate)
            {
                await _botClient.SendMessage(chatId, "Hãy dùng /unlink trong cuộc trò chuyện riêng với bot.", cancellationToken: ct);
                return;
            }

            var connections = await _connectionRepo.Query().Where(x => x.ChatId == chatId && !x.IsDeleted).ToListAsync(ct);
            foreach (var connection in connections)
            {
                connection.DeletedUser = $"telegram:{chatId}";
                await _connectionRepo.DeleteAsync(connection, ct);
            }

            await _botClient.SendMessage(chatId,
                connections.Count > 0 ? "✅ Đã gỡ liên kết Telegram này khỏi tài khoản ManageLife." : "Telegram này chưa liên kết với tài khoản nào.",
                cancellationToken: ct);
        }

        private async Task<string?> GetBotUsernameAsync(CancellationToken ct)
        {
            var cached = await _cache.TryGetValueAsync<string>(CacheSettings.TelegramBotUsername());
            if (cached.IsNotEmpty()) return cached;

            var me = await _botClient.GetMe(ct);
            if (me.Username.IsNotEmpty()) await _cache.SetAsync(me.Username!, CacheSettings.TelegramBotUsername());
            return me.Username;
        }

        private async Task LinkByCodeAsync(long chatId, bool isPrivate, string code, CancellationToken ct)
        {
            if (!isPrivate)
            {
                await _botClient.SendMessage(chatId, "Hãy mở liên kết trong cuộc trò chuyện riêng với bot.", cancellationToken: ct);
                return;
            }

            //NOTE: Kiểm tra định dạng trước khi tra cache (mã lấy từ nội dung tin nhắn)
            var userId = LinkCodePattern.IsMatch(code) ? await _cache.TryGetValueAsync<string>(CacheSettings.TelegramLinkCode(code)) : null;
            if (userId.IsEmpty())
            {
                await _botClient.SendMessage(chatId,
                    "❌ Mã liên kết không hợp lệ hoặc đã hết hạn. " + LinkGuide, cancellationToken: ct);
                return;
            }
            //NOTE: Mã dùng 1 lần
            await _cache.RemoveAsync(CacheSettings.TelegramLinkCode(code));

            var user = await _userRepo.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted && x.IsActive, ct);
            if (user == null)
            {
                await _botClient.SendMessage(chatId, "❌ Tài khoản không còn hoạt động.", cancellationToken: ct);
                return;
            }

            //NOTE: Một chat Telegram chỉ thuộc một tài khoản. Chat đang gắn với tài khoản khác thì từ chối
            // (không âm thầm gỡ: tránh người khác gửi liên kết của họ để chiếm chat của mình)
            var linkedToOther = await _connectionRepo.Query(true)
                .AnyAsync(x => x.ChatId == chatId && x.UserId != user.Id && !x.IsDeleted, ct);
            if (linkedToOther)
            {
                await _botClient.SendMessage(chatId,
                    "❌ Telegram này đang liên kết với một tài khoản ManageLife khác. Gửi /unlink để gỡ rồi mở lại liên kết từ app.",
                    cancellationToken: ct);
                return;
            }

            var existing = await _connectionRepo.FirstOrDefaultAsync(x => x.UserId == user.Id && !x.IsDeleted, ct);
            if (existing != null)
            {
                existing.ChatId = chatId;
                existing.UpdatedUser = user.UserName;
                await _connectionRepo.UpdateAsync(existing, ct);
            }
            else
            {
                await _connectionRepo.InsertAsync(new UserTelegramConnectionEntity
                {
                    Id = IdHelper.NewId(),
                    UserId = user.Id,
                    ChatId = chatId,
                    CreatedUser = user.UserName
                }, ct);
            }

            _logger.Info("Đã liên kết Telegram chat {chatId} với user {userId}", chatId, user.Id);
            //NOTE: Gửi text thường (không Markdown) để tên tài khoản có ký tự đặc biệt không làm lỗi tin nhắn
            await _botClient.SendMessage(chatId,
                $"✅ Đã liên kết Telegram với tài khoản {user.UserName}. Bạn sẽ nhận tóm tắt công việc mỗi sáng.",
                cancellationToken: ct);
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

                if (_webhookSecret.IsEmpty() || !WebhookSecretPattern.IsMatch(_webhookSecret!))
                {
                    msg = "TelegramSettings:WebhookSecret chưa cấu hình hoặc không hợp lệ (1–256 ký tự A-Z a-z 0-9 _ -)";
                    return Result.Error<string>(Result.DATA_INVALID.Code, msg);
                }

                //NOTE: Telegram gửi lại secret qua header X-Telegram-Bot-Api-Secret-Token để webhook xác thực
                await _botClient.SetWebhook(url, secretToken: _webhookSecret, allowedUpdates: [UpdateType.Message], cancellationToken: ct);
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
