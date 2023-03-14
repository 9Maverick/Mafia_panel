using Discord;
using Discord.WebSocket;
using Mafia_panel.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mafia_panel.Models;

public interface IDiscordClientModel
{
	SocketTextChannel AnnouncementChannel { get; set; }
	SocketGuildUser GameMaster { get; set; }
	SocketGuild Guild { get; set; }
	ObservableCollection<SocketTextChannel> GuildChannels { get; set; }
	ObservableCollection<SocketGuild> Guilds { get; set; }
	ObservableCollection<SocketGuildUser> GuildUsers { get; set; }
	SocketTextChannel LogChannel { get; set; }
	string Token { get; set; }
	public Task SendToLogChannel(string message);
	public Task SendToAnnounceChannel(string message);
	public void SendStatus(ObservableCollection<Player> Players);
	public void SendInitialStatus(ObservableCollection<Player> Players, IGameModeModel mode, ulong id = 970726830887804978); 
}

///// <summary>
///// Plug class without connection to Discord API
///// </summary>
//public class HollowDiscord : IDiscordClientModel
//{
//	public SocketTextChannel AnnouncementChannel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//	public SocketGuildUser GameMaster { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//	public SocketGuild Guild { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//	public ObservableCollection<SocketTextChannel> GuildChannels { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//	public ObservableCollection<SocketGuild> Guilds { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//	public ObservableCollection<SocketGuildUser> GuildUsers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//	public SocketTextChannel LogChannel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//	public string Token { get; set; }
//	public void SendToLogChannel(string message){}

//	public void SendInitialStatus(ObservableCollection<Player> Players, IGameModeModel mode, ulong id = 970726830887804978){}

//	public void SendToAnnounceChannel(string message){}

//	public void SendStatus(ObservableCollection<Player> Players){}
//}

class DiscordClientModel : ViewModelBase, IDiscordClientModel
{
	DiscordSocketClient client;

	/// <summary>
	/// Token of bot to login
	/// </summary>
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

	public void StartClient()
	{
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
	async Task GetGuilds() => Guilds = new ObservableCollection<SocketGuild>(client.Guilds);
	void GetGuildData()
	{
		GuildChannels = new ObservableCollection<SocketTextChannel>(Guild.TextChannels);
		GuildUsers = new ObservableCollection<SocketGuildUser>(Guild.Users);
	}
	public async Task SendToLogChannel(string message)
	{
		if (AnnouncementChannel == null) return;
		await LogChannel.SendMessageAsync(message);
	}
	public async Task SendToAnnounceChannel(string message)
	{
		if (AnnouncementChannel == null) return;
		await AnnouncementChannel.SendMessageAsync(message);
	}
	public async void SendInitialStatus(ObservableCollection<Player> Players, IGameModeModel mode, ulong id = 970726830887804978)
	{
		await SendToLogChannel("---------------------------------------\n" + "New game");
		await SendToLogChannel("Modificators: \n" +
			"Is defence stunning: " + mode.IsDefenseStunning.ToString() +
			"\nIs Godfather Can Check: " + mode.IsGodfatherCanCheck.ToString() +
			"\nIs Chief Limited Kills: " + mode.IsChiefLimitedKills.ToString() +
			"\nChief Limited Kills: " + mode.ChiefLimitedKills.ToString() +
			"\nIs Chief Cannot Kill Checked: " + mode.IsChiefCannotKillChecked.ToString());
		SendStatus(Players);
		foreach (var player in Players)
		{
			if (player.User == null) return;
			player.User.SendMessageAsync(Templates.RoleTemplates[player.Role]);
		}
	}
	public async void SendStatus(ObservableCollection<Player> Players)
	{
		await SendToLogChannel(" \n-------------------\nNew Status:");
		string message = "";
		foreach (Player player in Players) message += $"<@{player.Id}> - " + player.Role.ToString() + " - " + player.Status.ToString() + "\n";
		await SendToLogChannel(message);
	}

	/// <summary>
	/// Logging to trace
	/// </summary>
	/// <param name="msg"></param>
	/// <returns></returns>
	private Task Log(LogMessage msg)
	{
		Trace.WriteLine(msg);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Bot commands
	/// </summary>
	/// <param name="msg">Message to check for command</param>
	/// <returns></returns>
	private Task CommandsHandler(SocketMessage msg)
	{
		if (msg.Author.IsBot) return Task.CompletedTask;
		if (msg.Content.Contains("!target")) GameMaster.SendMessageAsync($"<@{msg.Author.Id}> targeted " + msg.Content.Substring(7));
		return Task.CompletedTask;
	}
}
