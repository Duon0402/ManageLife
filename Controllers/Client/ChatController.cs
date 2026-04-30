using ManageLife.Core;
using ManageLife.Contexts;
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
        public async Task<IActionResult> Index()
        {
            var res = await _userService.GetListUsersAsync();

            // Try multiple claim types to find the User ID
            var currentUserId = UserContext.GetUserId();

            _logger.Info($"Chat Index - UserID: {currentUserId}, Claims: {string.Join("|", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");

            var users = res.IsOk() ? res.Data.Where(x => x.Id != currentUserId).ToList() : new List<UserModel>();
            ViewBag.Users = users;
            ViewBag.CurrentUserId = currentUserId;
            return View();
        }

        [HttpPost("CreateOrGetPrivateRoom")]
        public async Task<Result<string>> CreateOrGetPrivateRoom([FromBody] PrivateRoomRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
                return Result.Error<string>("400", "UserId is required");

            var currentUserId = UserContext.GetUserId();
            if (string.IsNullOrEmpty(currentUserId))
                return Result.Error<string>("401", "Unauthorized");

            return await _chatService.CreateOrGetPrivateRoomAsync(currentUserId, request.UserId);
        }

        [HttpGet("{roomId}/messages")]
        public async Task<Result<List<ChatMessageModel>>> GetMessages(string roomId, [FromQuery] DateTime? before, [FromQuery] int pageSize = 50)
        {
            return await _chatService.GetMessagesAsync(roomId, before, pageSize);
        }
    }

    public class PrivateRoomRequest
    {
        public string UserId { get; set; }
    }
}
