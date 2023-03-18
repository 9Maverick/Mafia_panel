using Discord;
using Discord.WebSocket;
using Mafia_panel.Core;
using Mafia_panel.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mafia_panel.Models;

public interface IDiscordClientModel
{
	/// <summary>
	/// Token of bot to login
	/// </summary>
	string Token { get; set; }
	/// <summary>
	/// Server where the game will take place
	/// </summary>
	SocketGuild Guild { get; set; }
	/// <summary>
	/// The user to whom the game progress will be sent
	/// </summary>
	SocketGuildUser GameMaster { get; set; }
	/// <summary>
	/// The channel to which the game progress will be sent
	/// </summary>
	SocketTextChannel LogChannel { get; set; }
	/// <summary>
	/// Channel to which the game result will be sent
	/// </summary>
	SocketTextChannel AnnouncementChannel { get; set; }
	/// <summary>
	/// List of servers accessible to the bot
	/// </summary>
	ObservableCollection<SocketGuild> Guilds { get; set; }
	/// <summary>
	/// List of server users
	/// </summary>
	ObservableCollection<SocketGuildUser> GuildUsers { get; set; }
	/// <summary>
	/// List of server channels
	/// </summary>
	ObservableCollection<SocketTextChannel> GuildChannels { get; set; }
	/// <summary>
	/// Sends message to <see cref="LogChannel"/> or to <see cref="GameMaster"/> if able
	/// </summary>
	/// <param name="message">message to send</param>
	public Task SendLog(string message);
	/// <summary>
	/// Sends message to <see cref="AnnouncementChannel"/> if able
	/// </summary>
	/// <param name="message">message to send</param>
	public Task SendToAnnounceChannel(string message);
	/// <summary>
	/// Sends list of players with their roles and statuses via <see cref="SendLog(string)"/>
	/// </summary>
	public void SendStatus();
	/// <summary>
	/// Sends gamerules and calls <see cref="SendStatus(ObservableCollection{Player})"/>
	/// </summary>
	public void SendInitialStatus(); 
}

class DiscordClientModel : ViewModelBase, IDiscordClientModel
{
	DiscordSocketClient client;
	IPlayersViewModel _playersViewModel;
	IGameRulesModel _gameRules;

	string _token;
	public string Token
	{
		get => _token;
		set
		{
			SetProperty(ref _token, value);
			StartClient();
		}
	}
	SocketGuild _guild;
	public SocketGuild Guild
	{
		get => _guild;
		set
		{
			SetProperty(ref _guild, value);
			GetGuildData();
		}
	}
	SocketGuildUser _gameMaster;
	public SocketGuildUser GameMaster
	{
		get => _gameMaster;
		set => SetProperty(ref _gameMaster, value);
	}
	SocketTextChannel _logChannel;
	public SocketTextChannel LogChannel
	{
		get => _logChannel;
		set => SetProperty(ref _logChannel, value);
	}
	SocketTextChannel _announcementChannel;
	public SocketTextChannel AnnouncementChannel
	{
		get => _announcementChannel;
		set => SetProperty(ref _announcementChannel, value);
	}
	ObservableCollection<SocketGuild> _guilds;
	public ObservableCollection<SocketGuild> Guilds
	{
		get => _guilds;
		set => SetProperty(ref _guilds, value);
	}
	ObservableCollection<SocketGuildUser> _guildUsers;
	public ObservableCollection<SocketGuildUser> GuildUsers
	{
		get => _guildUsers;
		set => SetProperty(ref _guildUsers, value);
	}
	ObservableCollection<SocketTextChannel> _guildChannels;
	public ObservableCollection<SocketTextChannel> GuildChannels
	{
		get => _guildChannels;
		set => SetProperty(ref _guildChannels, value);
	}

	public DiscordClientModel(IGameRulesModel gameModeModel,IPlayersViewModel playersViewModel)
	{
		_playersViewModel = playersViewModel;
		_gameRules = gameModeModel;
	}

	/// <summary>
	/// Setting up the Discord client
	/// </summary>
	void StartClient()
	{
		if (string.IsNullOrEmpty(Token)) return;
		try
		{
			client = new DiscordSocketClient(new DiscordSocketConfig()
			{
				GatewayIntents = GatewayIntents.All,
				AlwaysDownloadUsers = true
			});

			client.MessageReceived += CommandsHandler;
			client.Log += Log;
			client.Ready += GetGuilds;

			client.LoginAsync(TokenType.Bot, _token);
			client.StartAsync();

		}
		catch (Exception ex)
		{
			Trace.WriteLine(DateTime.Now.ToString() + " " + ex.Message + "\n" + ex.ToString);
		}
	}
	/// <summary>
	/// Getting accessible guilds from client
	/// </summary>
	async Task GetGuilds() => Guilds = new ObservableCollection<SocketGuild>(client.Guilds);
	/// <summary>
	/// Getting users and channels from <see cref="Guild"/>
	/// </summary>
	void GetGuildData()
	{
		if (Guild == null) return;
		GuildChannels = new ObservableCollection<SocketTextChannel>(Guild.TextChannels);
		GuildUsers = new ObservableCollection<SocketGuildUser>(Guild.Users);
	}
	public async Task SendLog(string message)
	{
		if (LogChannel != null)
		{
			await LogChannel.SendMessageAsync(message);
			return;
		}
		if (GameMaster != null) 
		{
			await GameMaster.SendMessageAsync(message);
		}
	}
	public async Task SendToAnnounceChannel(string message)
	{
		if (AnnouncementChannel == null) return;
		await AnnouncementChannel.SendMessageAsync(message);
	}
	public async void SendStatus()
	{
		await SendLog(" \n-------------------\nNew Status:");
		string message = "";
		// Getting information for each player
		foreach (Player player in _playersViewModel.Players)
		{
			if (player.User != null)
			{
				message += $"<@{player.User.Id}> - " + player.Role.ToString() + " - " + player.Status.ToString() + "\n";
			}
		}
		await SendLog(message);
	}
	public async void SendInitialStatus()
	{
		await SendLog("---------------------------------------\n" + "New game");
		await SendLog("Modificators: \n" +
			"Is defence stunning: " + _gameRules.IsDefenseStunning.ToString() +
			"\nIs Godfather Can Check: " + _gameRules.IsGodfatherCanCheck.ToString() +
			"\nIs Chief Limited Kills: " + _gameRules.IsChiefLimitedKills.ToString() +
			"\nChief Limited Kills: " + _gameRules.ChiefLimitedKills.ToString() +
			"\nIs Chief Cannot Kill Checked: " + _gameRules.IsChiefCannotKillChecked.ToString());
		SendStatus();

		// Send to each player information about their roles
		foreach (var player in _playersViewModel.Players)
		{
			if (player.User == null) return;
			player.User.SendMessageAsync(Templates.RoleTemplates[player.Role]);
		}
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
	private Task CommandsHandler(SocketMessage msg)
	{
		if (msg.Author.IsBot) return Task.CompletedTask;
		if (msg.Content.Contains("!target"))
		{
			var message = $"<@{msg.Author.Id}> targeted " + msg.Content.Substring(7);
			SendLog(message);
		}
		return Task.CompletedTask;
	}
}
