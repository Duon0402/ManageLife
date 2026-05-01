using LinqKit;
using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatMessageRepository _repoChatMessage;
        private readonly IChatRoomMemberRepository _repoChatRoomMember;
        private readonly IChatRoomRepository _repoChatRoom;
        private readonly IChatRoomUserStateRepository _repoChatRoomUserState;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppLogger<ChatService> _logger;

        public ChatService(
            IChatMessageRepository repoChatMessage,
            IChatRoomMemberRepository repoChatRoomMember,
            IChatRoomRepository repoChatRoom,
            IChatRoomUserStateRepository repoChatRoomUserState,
            IUnitOfWork unitOfWork,
            IAppLogger<ChatService> logger)
        {
            _repoChatMessage = repoChatMessage;
            _repoChatRoomMember = repoChatRoomMember;
            _repoChatRoom = repoChatRoom;
            _repoChatRoomUserState = repoChatRoomUserState;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<string>> CreateOrGetPrivateRoomAsync(string user1, string user2, CancellationToken ct = default)
        {
            string msg;
            try
            {
                if (user1 == user2)
                {
                    return Result.Error<string>(Result.DATA_INVALID.Code, "Không thể tạo room với chính mình.");
                }

                var privateKey = GeneratePrivateKey(user1, user2);

                var existingRoomId = await _repoChatRoom.Query(true)
                    .Where(x => x.PrivateKey == privateKey)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                if (existingRoomId != null)
                {
                    return Result.Ok(existingRoomId);
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var room = new ChatRoomEntity
                    {
                        Type = RoomType.Private,
                        PrivateKey = privateKey
                    };

                    await _repoChatRoom.InsertAsync(room);

                    var members = new[]
                    {
                        new ChatRoomMemberEntity
                        {
                            RoomId = room.Id,
                            UserId = user1,
                            IsActive = true
                        },
                        new ChatRoomMemberEntity
                        {
                            RoomId = room.Id,
                            UserId = user2,
                            IsActive = true
                        }
                    };

                    await _repoChatRoomMember.BulkInsertAsync(members);

                    await _unitOfWork.CommitAsync();

                    return Result.Ok(room.Id);
                }
                catch (Exception ex) when (IsUniqueViolation(ex))
                {
                    await _unitOfWork.RollbackAsync();

                    var existingRoom = await _repoChatRoom.Query(true)
                        .Where(x => x.PrivateKey == privateKey)
                        .Select(x => x.Id)
                        .FirstAsync();

                    return Result.Ok(existingRoom);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<string>(msg, ex);
            }
        }

        public async Task<Result<List<ChatMessageModel>>> GetMessagesAsync(string roomId, DateTime? before, int pageSize, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var predicate = PredicateBuilder.New<ChatMessageEntity>(x => x.RoomId == roomId);

                if (before.HasValue)
                {
                    predicate = predicate.And(x => x.CreatedTime < before.Value);
                }

                var entities = await _repoChatMessage
                    .Query(true)
                    .Where(predicate)
                    .OrderByDescending(x => x.CreatedTime)
                    .Take(pageSize)
                    .ToListAsync();

                var models = entities.MapToList<ChatMessageModel>();

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<List<ChatMessageModel>>(msg, ex);
            }
        }

        public async Task<Result<int>> GetUnreadCountAsync(string roomId, string userId, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var state = await _repoChatRoomUserState
                    .FirstOrDefaultAsync(x =>
                        x.RoomId == roomId &&
                        x.UserId == userId);

                var count = 0;
                if (state?.LastReadAt == null)
                {
                    count = await _repoChatMessage
                        .Query(true)
                        .CountAsync(x => x.RoomId == roomId);

                    return Result.Ok(count);
                }

                count = await _repoChatMessage
                    .Query(true)
                    .CountAsync(x =>
                        x.RoomId == roomId &&
                        x.CreatedTime > state.LastReadAt);

                return Result.Ok(count);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<int>(msg, ex);
            }
        }

        public async Task<Result> MarkAsReadAsync(string userId, string roomId, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var state = await _repoChatRoomUserState
                    .FirstOrDefaultAsync(x =>
                        x.RoomId == roomId &&
                        x.UserId == userId);

                if (state == null)
                {
                    state = new ChatRoomUserStateEntity
                    {
                        RoomId = roomId,
                        UserId = userId,
                        LastReadAt = DateTimeHelper.UtcNow(),
                    };

                    var b = await _repoChatRoomUserState.InsertAsync(state);

                    if (!b)
                    {
                        msg = TranslationKey.Common.Message.CreateError;
                        _logger.Debug(msg);
                        return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                    }
                }
                else
                {
                    state.LastReadAt = DateTimeHelper.UtcNow();
                    var b = await _repoChatRoomUserState.UpdateAsync(state);
                    if (!b)
                    {
                        msg = TranslationKey.Common.Message.UpdateError;
                        _logger.Debug(msg);
                        return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                    }
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result<ChatMessageModel>> SendMessageAsync(string roomId, string senderId, string content, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var roomMember = await _repoChatRoomMember.FirstOrDefaultAsync(
                    x => x.IsActive &&
                    x.UserId == senderId &&
                    x.RoomId == roomId);

                if (roomMember == null)
                {
                    msg = "Không thể gửi tin nhắn";
                    _logger.Debug(msg);
                    return Result.Error<ChatMessageModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var entity = new ChatMessageEntity
                {
                    RoomId = roomId,
                    SenderId = senderId,
                    Content = content
                };

                var b = await _repoChatMessage.InsertAsync(entity);

                if (!b)
                {
                    msg = "Không thể gửi tin nhắn";
                    _logger.Debug(msg);
                    return Result.Error<ChatMessageModel>(Result.DATA_NOT_CREATE.Code, msg);
                }

                return Result.Ok(entity.MapTo<ChatMessageModel>());
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<ChatMessageModel>(msg, ex);
            }
        }

        // Check MySQL "Duplicate entry" — SQL Server strings removed (wrong driver)
        private static bool IsUniqueViolation(Exception ex)
        {
            return ex is DbUpdateException dbEx &&
                   dbEx.InnerException?.Message.Contains("Duplicate entry") == true;
        }

        private static string GeneratePrivateKey(string user1, string user2)
        {
            if (user1.IsEmpty())
                throw new ArgumentException("user1 is required");

            if (user2.IsEmpty())
                throw new ArgumentException("user2 is required");

            return string.CompareOrdinal(user1, user2) < 0
                ? $"{user1}:{user2}"
                : $"{user2}:{user1}";
        }

        public async Task<bool> IsMemberAsync(string roomId, string userId, CancellationToken ct = default)
        {
            return await _repoChatRoomMember.Query(true)
                .AnyAsync(x =>
                    x.RoomId == roomId &&
                    x.UserId == userId &&
                    x.IsActive);
        }
    }
}
