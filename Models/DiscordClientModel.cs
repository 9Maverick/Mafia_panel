using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Mafia_panel.Models;

public interface IDiscordClientModel
{
	public void Send(string message, ulong id= 970726830887804978);
	public void SendToUserById(ulong id, string message);
	public void SendStart(string message, ObservableCollection<Player> Players);
	public void SendStatus(ObservableCollection<Player> Players, ulong id = 970726830887804978);
	public void ConfigurePlayers(ObservableCollection<Player> Players);
	public void SendInitialStatus(ObservableCollection<Player> Players, IGameModeModel mode, ulong id = 970726830887804978);
}

/// <summary>
/// Plug class without connection to Discord API
/// </summary>
public class HollowDiscord : IDiscordClientModel
{
	public void ConfigurePlayers(ObservableCollection<Player> Players){}
	public void Send(string message, ulong id = 970726830887804978){}

	public void SendInitialStatus(ObservableCollection<Player> Players, IGameModeModel mode, ulong id = 970726830887804978){}

	public void SendStart(string message, ObservableCollection<Player> Players){}

	public void SendStatus(ObservableCollection<Player> Players, ulong id = 970726830887804978){}

	public void SendToUserById(ulong id, string message){}
}
class DiscordClientModel : IDiscordClientModel
{
	#region Variables and Properties
	/// <summary>
	/// Client of discord bot
	/// </summary>
	DiscordSocketClient client;

	/// <summary>
	/// Token of bot to login
	/// </summary>
	string token = "";

	SocketGuild _guild;

	#endregion

	#region Functions
	/// <summary>
	/// Constructor, witch creating client
	/// </summary>
	public DiscordClientModel()
	{
		StartClient();
	}
	async void StartClient()
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
			client.Ready += GetGuildAsync;

			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

		}
		catch (Exception ex)
		{
			Trace.WriteLine(DateTime.Now.ToString() + " " + ex.Message + "\n" + ex.ToString);
		}
	}
	async Task GetGuildAsync()
	{
		_guild = client.GetGuild(908016523736645712);
		await _guild.DownloadUsersAsync();
	}
	public void ConfigurePlayers(ObservableCollection<Player> Players)
	{
		foreach(Player player in Players)
		{
			var user = _guild.GetUser(player.Id);
			player.Name = user.Nickname ?? user.Username;
			player.User = user;
		}
	}
	public void Send(string message, ulong id = 970726830887804978)
	{
		SocketChannel channel = client.GetChannel(id);
		_ = channel ?? throw new InvalidOperationException("Cannot find channel by ID");
		((SocketTextChannel)channel).SendMessageAsync(message);
		Thread.Sleep(1000);
	}
	public void SendToUserById(ulong id, string message)
	{
		var user = client.GetUserAsync(id).Result;
		user.SendMessageAsync(message);
	}
	public void SendStart(string message, ObservableCollection<Player> Players)
	{
		throw new NotImplementedException();
	}
	public void SendInitialStatus(ObservableCollection<Player> Players, IGameModeModel mode, ulong id = 970726830887804978)
	{
		Send("---------------------------------------\n" + "New game", id);
		Send("Modificators: \n" +
			"Is defence stunning: " + mode.IsDefenseStunning.ToString() +
			"\nIs Curator Can Check: " + mode.IsCuratorCanCheck.ToString() +
			"\nIs Chief Limited Kills: " + mode.IsChiefLimitedKills.ToString() +
			"\nChief Limited Kills: " + mode.ChiefLimitedKills.ToString() +
			"\nIs Chief Cannot Kill Checked: " + mode.IsChiefCannotKillChecked.ToString(), id);
		SendStatus(Players, id);
		foreach (var player in Players)player.User.SendMessageAsync(Templates.RoleTemplates[player.Role]);
	}
	public void SendStatus(ObservableCollection<Player> Players, ulong id = 970726830887804978)
	{
		Send(" \n-------------------\nNew Status:", id);
		string message = "";
		foreach(Player player in Players)message += $"<@{player.Id}> - " + player.Role.ToString() + " - " + player.Status.ToString() + "\n";
		Send(message,id);
	}
	#endregion

	#region Handlers and Delegates
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
		if (msg.Content.Contains("!target")) SendToUserById(413693028713365514, $"<@{msg.Author.Id}> targeted " + msg.Content.Substring(7));
		return Task.CompletedTask;
	}
	#endregion
}
