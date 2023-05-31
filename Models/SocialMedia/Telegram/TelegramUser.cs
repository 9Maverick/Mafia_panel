using Mafia_panel.Core;
using Mafia_panel.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Mafia_panel.Models.SocialMedia.Telegram;

public class TelegramUser : NotifyPropertyChanged, ISocialMediaUser
{
	Chat _chat;
	TelegramBotClient _client;
	public string Name => _chat.Username;
	public long Id => _chat.Id;

	public TelegramUser(Chat chat, TelegramBotClient client)
	{
		 _chat = chat;
		 _client = client;
	}

	public void SendMessage(string message)
	{
		var id = new ChatId(Id);
		_client.SendTextMessageAsync(id, message);
	}
}
