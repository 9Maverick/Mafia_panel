using Mafia_panel.Core;
using Mafia_panel.Interfaces;
using Mafia_panel.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Mafia_panel.Models.SocialMedia.Telegram;

public class TelegramClientModel : NotifyPropertyChanged, ISocialMediaProviderWithSettings
{
	const string token = "Token";
	const string logChannel = "Log";
	const string chatChannel = "Chat";

	const string joinGame = "/joingame";
	const string quitGame = "/quitgame";
	const string vote = "/vote";
	const string action = "/action";
	const string alternativeAction = "/actionalternative";
	const string addChat = "/addchat";

	NightViewModel _nightViewModel;
	IPlayersViewModel _playersViewModel;
	TelegramBotClient _client;
	public string Name => "Telegram";

	ReadOnlyDictionary<string, SocialMediaSetting> _settings;
	public ReadOnlyDictionary<string, SocialMediaSetting> Settings => _settings;

	bool _isActive;
	public bool IsActive
	{
		get => _isActive;
		set => SetValue(ref _isActive, value);
	}
	public TelegramClientModel(IPlayersViewModel playersViewModel)
	{
		_playersViewModel = playersViewModel;
		var settings = new Dictionary<string, SocialMediaSetting>
		{
			{ token,        new SocialMediaSetting( typeof(string),ControlType.TextBox,    StartClient )},
			{ logChannel,   new SocialMediaSetting( typeof(KeyValuePair<string, long>),  ControlType.ComboBox    )},
			{ chatChannel,  new SocialMediaSetting( typeof(KeyValuePair<string, long>),  ControlType.ComboBox    )}
		};
		_settings = settings.AsReadOnly();
	}

	void StartClient()
	{
		var botToken = GetSettingValue<string>(token);
		if (string.IsNullOrEmpty(botToken)) return;
		try
		{
			_nightViewModel = App.Host.Services.GetRequiredService<NightViewModel>();
			_client = new TelegramBotClient(botToken);
			_client.StartReceiving(UpdateHandler, ErrorHandler);
		}
		catch (Exception ex)
		{
			Trace.WriteLine(DateTime.Now.ToString() + " " + ex.Message + "\n" + ex.ToString);
		}
	}

	Task ErrorHandler(ITelegramBotClient client, Exception e, CancellationToken cancellationToken)
	{
		Trace.WriteLine(e.Message);
		return Task.CompletedTask;
	}

	Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
	{
		if (update.Type != UpdateType.Message) return Task.CompletedTask;
		var text = update.Message.Text;
		if (string.IsNullOrEmpty(text) || !text.StartsWith("/")) return Task.CompletedTask;
		var splitedText = text.Split(" ");
		var keyword = splitedText[0];
		var args = "";
		if (splitedText.Length > 1) 
		{ 
			args = splitedText[1];
		}
		switch(keyword)
		{
			case joinGame:
				JoinQuitCommandHandler(client, update, AddPlayer);
				break;
			case quitGame:
				JoinQuitCommandHandler(client, update, RemovePlayer);
				break;
			case vote:
				VoteCommandHandler(client,update, args); 
				break;
			case action:
				ActionCommandHandler(client,update, args, false);
				break;
			case alternativeAction:
				ActionCommandHandler(client, update, args, true);
				break;
			case addChat:
				AddChatCommandHandler(client, update);
				break;
		}
		return Task.CompletedTask;
	}
	void AddChatCommandHandler(ITelegramBotClient client, Update update)
	{
		Settings.TryGetValue(logChannel, out var log);
		Settings.TryGetValue(chatChannel, out var chat);
		var cht = update.Message.Chat;
		KeyValuePair<string, long> pair;
		if (cht.Type == ChatType.Private)
		{
			pair = new KeyValuePair<string, long>(cht.Username, cht.Id);
		}
		else
		{
			pair = new KeyValuePair<string, long>(cht.Title, cht.Id);
		}
		App.Current.Dispatcher.Invoke(delegate
		{
			log.Source.Add(pair);
			chat.Source.Add(pair);
		});
	}
	void JoinQuitCommandHandler(ITelegramBotClient client, Update update, Func<Chat, string> addRemove)
	{
		var chat = update.Message.Chat;
		client.SendTextMessageAsync(new ChatId(chat.Id), addRemove(chat));
	}
	void ActionCommandHandler(ITelegramBotClient client, Update update, string args, bool isAlternative)
	{
		var chatId = update.Message.Chat.Id;
		if (!int.TryParse(args, out var target))
		{
			client.SendTextMessageAsync(new ChatId(chatId), "Invalid data");
			return;
		}
		target--;
		var player = _playersViewModel.GetActivePlayerByUserId(chatId);
		if (player == null)
		{
			client.SendTextMessageAsync(new ChatId(chatId), "You must be in the game to vote");
			return;
		}
		if (!player.CanAct)
		{
			client.SendTextMessageAsync(new ChatId(chatId), "It's not your turn");
			return;
		}
		if (target < 0 || target >= _playersViewModel.ActivePlayers.Count)
		{
			client.SendTextMessageAsync(new ChatId(chatId), "Target player out of range");
			return;
		}
		_nightViewModel.TargetPlayer = _playersViewModel.ActivePlayers[target];
		if (!isAlternative)
		{
			App.Current.Dispatcher.Invoke(delegate
			{
				_nightViewModel.ActionCommand.Execute(null);
			});
		}
		else
		{
			App.Current.Dispatcher.Invoke(delegate
			{
				_nightViewModel.AltenativeActionCommand.Execute(null);
			});
		}
		client.SendTextMessageAsync(new ChatId(chatId), "Action applied");
	}
	void VoteCommandHandler(ITelegramBotClient client, Update update, string args)
	{
		var chatId = update.Message.Chat.Id;
		if (!int.TryParse(args,out var target))
		{
			client.SendTextMessageAsync(new ChatId(chatId), "Invalid data");
			return;
		}
		target--;
		var player = _playersViewModel.GetActivePlayerByUserId(chatId);
		if (player == null)
		{
			client.SendTextMessageAsync(new ChatId(chatId), "You must be in the game to vote");
			return;
		}
		if (!player.CanVote)
		{
			client.SendTextMessageAsync(new ChatId(chatId), "You cannot vote right now");
			return;
		}
		if (target < 0 || target >= _playersViewModel.ActivePlayers.Count)
		{
			client.SendTextMessageAsync(new ChatId(chatId), "Target player out of range");
			return;
		}
		var targetPlayer = _playersViewModel.ActivePlayers[target];
		targetPlayer.Votes++;
		player.CanVote = false;
		client.SendTextMessageAsync(new ChatId(chatId), $"You voted for {targetPlayer.Name}");
	}
	string AddPlayer(Chat chat)
	{
		if (_playersViewModel.GetPlayerByUserId(chat.Id) != null)
		{
			return $"{chat.Username} already in the game";
		}
		App.Current.Dispatcher.Invoke(delegate
		{
			_playersViewModel.Players.Add
				(new Player(new TelegramUser(chat,_client)));
		});
		return $"{chat.Username} added to the game";
	}
	string RemovePlayer(Chat chat)
	{
		if (_playersViewModel.GetPlayerByUserId(chat.Id) == null)
		{
			return $"{chat.Username} not found";
		}
		var player = _playersViewModel.GetPlayerByUserId(chat.Id);
		var activePlayer = _playersViewModel.GetActivePlayerByUserId(chat.Id);
		App.Current.Dispatcher.Invoke(delegate
		{
			_playersViewModel.ActivePlayers.Remove(activePlayer);
			_playersViewModel.Players.Remove(player);
		});
		return $"{chat.Username} deleted from the game";
	}
	T? GetSettingValue<T>(string key)
	{
		Settings.TryGetValue(key, out var setting);
		return (T?)setting.Value;
	}
	public async Task SendLog(string message)
	{
		var log = GetSettingValue<KeyValuePair<string,long>>(logChannel);
		await _client.SendTextMessageAsync(new ChatId(log.Value), message);
	}

	public async Task SendToChat(string message)
	{
		var chat = GetSettingValue<KeyValuePair<string, long>>(chatChannel);
		await _client.SendTextMessageAsync(new ChatId(chat.Value), message);
	}
}
