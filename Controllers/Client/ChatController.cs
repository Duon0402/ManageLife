using ManageLife.Core;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    [Authorize]
    [Route("Chat")]
    public class ChatController : WebClientControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly IAppLogger<ChatController> _logger;

        public ChatController(IChatService chatService, IUserService userService, IAppLogger<ChatController> logger)
        {
            _chatService = chatService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var res = await _userService.GetListUsersAsync(ct);

            var currentUserId = User.GetUserId();

            _logger.Info($"Chat Index - UserID: {currentUserId}, Claims: {string.Join("|", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");

            return View();
        }

        [HttpPost("CreateOrGetPrivateRoom")]
        public async Task<Result<string>> CreateOrGetPrivateRoom([FromBody] PrivateRoomRequest request, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(request.UserId))
                return Result.Error<string>("400", "UserId is required");

            var currentUserId = User.GetUserId();
            if (string.IsNullOrEmpty(currentUserId))
                return Result.Error<string>("401", "Unauthorized");

            return await _chatService.CreateOrGetPrivateRoomAsync(currentUserId, request.UserId, ct);
        }

        [HttpGet("{roomId}/messages")]
        public async Task<Result<List<ChatMessageModel>>> GetMessages(string roomId, [FromQuery] DateTime? before, [FromQuery] int pageSize = 50, CancellationToken ct = default)
        {
            return await _chatService.GetMessagesAsync(roomId, before, pageSize, ct);
        }
    }

    public class PrivateRoomRequest
    {
        public string UserId { get; set; }
    }
}
