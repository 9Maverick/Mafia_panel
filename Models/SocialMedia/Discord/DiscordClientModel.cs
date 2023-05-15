using Discord;
using Discord.Net;
using Discord.WebSocket;
using Mafia_panel.Core;
using Mafia_panel.Interfaces;
using Mafia_panel.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
namespace Mafia_panel.Models.SocialMedia.Discord;

public class DiscordClientModel : ViewModelBase, ISocialMediaProviderWithSettings
{
	const string token = "Token";
	const string guild = "Guild";
	const string gameMaster = "Game master";
	const string logChannel = "Log";
	const string chatChannel = "Chat";

	const string joinGame = "join-game";
	const string quitGame = "quit-game";

	DiscordSocketClient _client;
	IPlayersViewModel _playersViewModel;

	bool _isActive;
	public bool IsActive
	{
		get => _isActive;
		set => SetValue(ref _isActive, value);
	}

	public string Name => "Discord";
	ReadOnlyDictionary<string, SocialMediaSetting> _settings;
	public ReadOnlyDictionary<string, SocialMediaSetting> Settings => _settings;

	public DiscordClientModel(IPlayersViewModel playersViewModel)
	{
		_playersViewModel = playersViewModel;
		var settings = new Dictionary<string, SocialMediaSetting>
		{
			{ token,        new SocialMediaSetting( typeof(string),             ControlType.TextBox,    StartClient )},
			{ guild,        new SocialMediaSetting( typeof(SocketGuild),        ControlType.ComboBox,   GetGuildData)},
			{ gameMaster,   new SocialMediaSetting( typeof(SocketGuildUser),    ControlType.ComboBox    )},
			{ logChannel,   new SocialMediaSetting( typeof(SocketTextChannel),  ControlType.ComboBox    )},
			{ chatChannel,  new SocialMediaSetting( typeof(SocketTextChannel),  ControlType.ComboBox    )}
		};
		_settings = settings.AsReadOnly();
	}
	T? GetSettingValue<T>(string key)
	{
		Settings.TryGetValue(key, out var setting);
		return (T?)setting.Value;
	}
	void UpdateSettingSource(string key, ObservableCollection<object> source)
	{
		Settings.TryGetValue(key, out var setting);
		setting.Source = source;
	}

	/// <summary>
	/// Setting up the Discord client
	/// </summary>
	void StartClient()
	{
		var token = GetSettingValue<string>(DiscordClientModel.token);
		if (string.IsNullOrEmpty(token)) return;
		try
		{
			_client = new DiscordSocketClient(new DiscordSocketConfig()
			{
				GatewayIntents = GatewayIntents.All,
				AlwaysDownloadUsers = true
			});

			_client.MessageReceived += CommandsHandler;
			_client.SlashCommandExecuted += SlashCommandsHandler;
			_client.Log += Log;
			_client.Ready += GetGuilds;
			_client.Ready += ConfigureCommands;

			_client.LoginAsync(TokenType.Bot, token);
			_client.StartAsync();

		}
		catch (Exception ex)
		{
			Trace.WriteLine(DateTime.Now.ToString() + " " + ex.Message + "\n" + ex.ToString);
		}
	}
	/// <summary>
	/// Adds commands to the bot
	/// </summary>
	async Task ConfigureCommands()
	{
		var joinCommand = new SlashCommandBuilder()
			.WithName(joinGame)
			.WithDescription("Adds you or specified user to the game")
			.AddOption("user", ApplicationCommandOptionType.User, "Users to be added to the game", isRequired: false);
		var quitCommand = new SlashCommandBuilder()
			.WithName(quitGame)
			.WithDescription("Removes you or specified user from the game")
			.AddOption("user", ApplicationCommandOptionType.User, "Users to be removed from the game", isRequired: false);

		try
		{
			await _client.CreateGlobalApplicationCommandAsync(joinCommand.Build());
			await _client.CreateGlobalApplicationCommandAsync(quitCommand.Build());
		}
		catch (ApplicationCommandException exception)
		{
			Trace.WriteLine(exception.Message);
		}
	}
	/// <summary>
	/// Getting accessible guilds from client
	/// </summary>
	async Task GetGuilds() => UpdateSettingSource(guild, new ObservableCollection<object>(_client.Guilds));

	/// <summary>
	/// Getting users and channels from <see cref="guild"/>
	/// </summary>
	void GetGuildData()
	{
		var guild = GetSettingValue<SocketGuild>(DiscordClientModel.guild);
		if (guild == null) return;
		UpdateSettingSource(logChannel, new ObservableCollection<object>(guild.TextChannels));
		UpdateSettingSource(chatChannel, new ObservableCollection<object>(guild.TextChannels));
		UpdateSettingSource(gameMaster, new ObservableCollection<object>(guild.Users));
	}
	public async Task SendLog(string message)
	{
		var log = GetSettingValue<SocketTextChannel>(logChannel);
		var master = GetSettingValue<SocketGuildUser>(gameMaster);

		if (log != null)
		{
			await log.SendMessageAsync(message);
		}
		if (master != null)
		{
			await master.SendMessageAsync(message);
		}
	}
	public async Task SendToChat(string message)
	{
		var chat = GetSettingValue<SocketTextChannel>(chatChannel);
		if (chat == null) return;
		await chat.SendMessageAsync(message);
	}

	/// <summary>
	/// Logging to trace
	/// </summary>
	/// <param name="msg"></param>
	private Task Log(LogMessage msg)
	{
		Trace.WriteLine(msg);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Bot commands
	/// </summary>
	/// <param name="msg">Message to check for command</param>
	Task CommandsHandler(SocketMessage msg)
	{
		if (msg.Source != MessageSource.User) return Task.CompletedTask;
		//if(msg.Channel == ChatChannel)
		//{

		//}

		if(msg.Content.Contains("!join"))
		{
			if (msg.MentionedUsers.Count > 0)
			{
				msg.MentionedUsers.ToList()
					.ForEach(user =>
						msg.Channel.SendMessageAsync(AddPlayer(user))
					);
			}
			else
			{
				msg.Channel.SendMessageAsync(AddPlayer(msg.Author));
			}
			return Task.CompletedTask;
		}

		if(msg.Content.Contains("!quit"))
		{
			if (msg.MentionedUsers.Count > 0)
			{
				msg.MentionedUsers.ToList()
					.ForEach(user =>
						msg.Channel.SendMessageAsync(RemovePlayer(user))
					);
			}
			else
			{
				msg.Channel.SendMessageAsync(RemovePlayer(msg.Author));
			}
			return Task.CompletedTask;
		}

		if (msg.Content.Contains("!target"))
		{
			var message = $"<@{msg.Author.Id}> targeted " + msg.Content.Substring(7);
			SendLog(message);
		}
		return Task.CompletedTask;
	}
	async Task SlashCommandsHandler(SocketSlashCommand command)
	{
		switch(command.Data.Name)
		{
			case joinGame:
				long id;
				if (command.Data.Options.Count > 0)
				{
					command.Data.Options
						.Where(option => option.Type == ApplicationCommandOptionType.User)
						.ToList()
						.ForEach(option => command.RespondAsync(AddPlayer((SocketUser)option.Value), ephemeral: true));
				}
				else
				{
					command.RespondAsync(AddPlayer(command.User), ephemeral: true);
				}
				break;
			case quitGame:
				if (command.Data.Options.Count > 0)
				{
					command.Data.Options
						.Where(option => option.Type == ApplicationCommandOptionType.User)
						.ToList()
						.ForEach(option => command.RespondAsync(RemovePlayer((SocketUser)option.Value), ephemeral: true));
				}
				else
				{
					command.RespondAsync(RemovePlayer(command.User), ephemeral: true);
				}
				break;
		}
	}
	string AddPlayer(SocketUser user)
	{
		if (_playersViewModel.GetPlayerByUserId((long)user.Id) != null) 
		{
			return $"{user.Username} already in the game";
		}
		App.Current.Dispatcher.Invoke(delegate
		{
			_playersViewModel.Players.Add
				(new Player(new DiscordUser(user)));
		});
		return $"{user.Username} added to the game";
	}
	string RemovePlayer(SocketUser user)
	{
		if (_playersViewModel.GetPlayerByUserId((long)user.Id) == null)
		{
			return $"{user.Username} not found";
		}
		App.Current.Dispatcher.Invoke(delegate
		{
			_playersViewModel.Players.Remove(
				_playersViewModel.GetPlayerByUserId((long)user.Id)
			);
		});
		return $"{user.Username} deleted from the game";
	}
}
