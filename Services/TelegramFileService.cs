using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Helpers;
using ManageLife.Models;
using ManageLife.Repositories;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ManageLife.Services
{
    public class TelegramFileService : ServiceBase
    {
        private readonly IConfiguration _config;
        private readonly string _botToken;
        private readonly string? _chatId;
        private readonly TelegramBotClient _botClient;
        private readonly FileRepository _repo;

        public TelegramFileService(AppDbContext context, IConfiguration config) : base(context)
        {
            _config = config;
            _botToken = _config["TelegramSettings:BotToken"] ?? string.Empty;
            _chatId = _config["TelegramSettings:ChatId"];
            _botClient = new TelegramBotClient(_botToken);
            _repo = new FileRepository(context);
        }

        public async Task<Result<FileModel>> UploadFileAsync(IFormFile file, string? caption = null)
        {
            string msg;
            bool b;
            try
            {
                if (file == null || file.Length == 0)
                {
                    msg = "File không hợp lệ";
                    return Result.Error<FileModel>(Result.DATA_INVALID.Code, msg);
                }

                if (_chatId == null)
                {
                    msg = "Không lấy được ChatId";
                    return Result.Error<FileModel>(Result.DATA_INVALID.Code, msg);
                }

                using var stream = file.OpenReadStream();
                var inputFile = new InputFileStream(stream, file.FileName);
                var fileType = TelegramFileTypeHelper.Detect(file);

                var message = await SendFileToTelegramAsync(fileType, inputFile, caption);
                if (message == null)
                {
                    msg = "Không nhận được thông tin Message từ Telegram";
                    return Result.Error<FileModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }
                var fileInfo = ExtractTelegramFileInfo(message);
                if (fileInfo == null)
                {
                    msg = "Không nhận được thông tin File từ Telegram";
                    return Result.Error<FileModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = GetFileModelFormTelegramMessage(file, fileInfo);
                var entity = model.MapTo<FileEntity>();

                b = await _repo.InsertAsync(entity);

                if (!b)
                {
                    msg = "Không thể lưu thông tin File";
                    return Result.Error<FileModel>(Result.DATA_NOT_CREATE.Code, msg);
                }

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi upload File";
                return Result.Exception<FileModel>(msg, ex);
            }
        }

        public async Task<Result<string>> GetFileUrlByFileIdAsync(string fileId)
        {
            string msg;
            try
            {
                var file = await _botClient.GetFile(fileId);
                var filePath = file?.FilePath;
                if (file == null || filePath.IsEmpty())
                {
                    msg = "Không tìm thấy File";
                    return Result.Error<string>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var fileUrl = $"https://api.telegram.org/file/bot{_botToken}/{file.FilePath}";
                return Result.Ok(fileUrl);
            }
            catch (Exception ex)
            {
                msg = "Đã có lỗi xảy ra khi tải file";
                return Result.Exception<string>(msg, ex);
            }
        }

        #region Private Method
        private async Task<Message> SendFileToTelegramAsync(TelegramFileType fileType, InputFileStream inputFile, string? caption)
        {
            return fileType switch
            {
                TelegramFileType.Photo => await _botClient.SendPhoto(_chatId!, inputFile, caption),
                TelegramFileType.Video => await _botClient.SendVideo(_chatId!, inputFile, caption),
                TelegramFileType.Audio => await _botClient.SendAudio(_chatId!, inputFile, caption),
                TelegramFileType.Animation => await _botClient.SendAnimation(_chatId!, inputFile, caption),
                _ => await _botClient.SendDocument(_chatId!, inputFile, caption)
            };
        }

        private FileModel GetFileModelFormTelegramMessage(IFormFile file, object fileInfo)
        {
            var fileId = fileInfo.GetType().GetProperty("FileId")?.GetValue(fileInfo)?.ToString() ?? string.Empty;
            var fileName = fileInfo.GetType().GetProperty("FileName")?.GetValue(fileInfo)?.ToString() ?? file.FileName;
            var mime = fileInfo.GetType().GetProperty("MimeType")?.GetValue(fileInfo)?.ToString() ?? file.ContentType;
            var size = (long?)fileInfo.GetType().GetProperty("FileSize")?.GetValue(fileInfo) ?? file.Length;
            var extension = Path.GetExtension(file.FileName) ?? string.Empty;

            var model = new FileModel
            {
                Id = IdHeper.NewId(),
                FileId = fileId,
                FileName = fileName,
                FileType = mime,
                FileSize = size,
                Extension = extension
            };

            return model;
        }

        private object? ExtractTelegramFileInfo(Message message)
        {
            return (object?)message.Document
                ?? (object?)message.Photo?.LastOrDefault()
                ?? (object?)message.Video
                ?? (object?)message.Audio
                ?? (object?)message.Animation;
        }
        #endregion
    }
}
