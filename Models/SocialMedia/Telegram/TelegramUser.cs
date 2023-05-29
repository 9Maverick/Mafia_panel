using Mafia_panel.Core;
using Mafia_panel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Mafia_panel.Models.SocialMedia.Telegram;

public class TelegramUser : NotifyPropertyChanged, ISocialMediaUser
{
	Chat _chat;
	TelegramBotClient _client;
	string _name;
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
