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

		public async Task SendMessageAsync(string message)
		{
			try
			{
				await _botClient.SendTextMessageAsync(_chatId, message);
			}
			catch (Exception ex)
			{
				var msg = ex.ToString();
			}
		}
	}
}
