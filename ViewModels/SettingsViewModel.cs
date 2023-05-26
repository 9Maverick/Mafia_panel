using Mafia_panel.Models;
using Mafia_panel.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Mafia_panel.Models.SocialMedia;
using Mafia_panel.Models.SocialMedia.Discord;

namespace Mafia_panel.ViewModels;

internal class SettingsViewModel : PhaseViewModel
{
	IPlayersViewModel _playersViewModel;
	IMainViewModel _windowModel;

	DiscordClientModel _discordClient;
	public DiscordClientModel DiscordClientModel
	{
		get => _discordClient;
		set => SetValue(ref _discordClient, value);
	}
	SocialMediaProvider _socialMediaProvider;
	public SocialMediaProvider SocialMediaProvider
	{
		get => _socialMediaProvider;
		set => SetValue(ref _socialMediaProvider, value);
	}
	private Player _selectedPlayer;
	public Player SelectedPlayer
	{
		get => _selectedPlayer;
		set => SetValue(ref _selectedPlayer, value);
	}
	public ObservableCollection<Player> Players => _playersViewModel.Players;

	IGameRulesModel _mode;
	public IGameRulesModel Mode
	{
		get => _mode;
		set => SetValue(ref _mode, value);
	}
	private bool _isRolesGiven = false;
	public bool IsRolesGiven
	{
		get => _isRolesGiven;
		set => SetValue(ref _isRolesGiven, value);
	}

	public SettingsViewModel(IPlayersViewModel playersViewModel, IGameRulesModel mode, SocialMediaProvider socialMediaProvider, IMainViewModel windowModel)
	{
		_playersViewModel = playersViewModel;
		_mode = mode;
		_socialMediaProvider = socialMediaProvider;
		_windowModel = windowModel;
	}

	private Command _addPlayerCommand;
	public Command AddPlayerCommand
	{
		get => _addPlayerCommand ??	(_addPlayerCommand = new Command(obj =>
		{
			Players.Add(new Player());
			IsRolesGiven = false;
		}));
	}
	private Command _removePlayerCommand;
	public Command RemovePlayerCommand
	{
		get => _removePlayerCommand ?? (_removePlayerCommand = new Command(obj => 
		{
			if (!Players.Any()) return;
			if(SelectedPlayer != null) 
			{
				Players.Remove(SelectedPlayer);
				return;
			}
			Players.Remove(Players.Last());
			IsRolesGiven = false;
		}));
	}

	private Command _addRolesCommand;
	/// <summary>
	/// Add <see cref="PlayerRole"/> to each <see cref="Player"/>
	/// </summary>
	public Command AddRolesCommand
	{
		get => _addRolesCommand ??(_addRolesCommand = new Command(obj =>
		{
			if (Players.Count < 3) return;
			
			// Setting role of everyone to Civilian
			foreach (Player player in Players) player.Role = PlayerRole.Civilian;

			// Proportionally adding some mafioso
			int mafiozoCount = Players.Count / 3;
			for (int i = 0; i < mafiozoCount; i++)
			{
				SetPlayerRole(PlayerRole.Mafioso);
			}

			// Setting additional roles depending on number of players
			if (!(Players.Count < 6)) SetPlayerRole(PlayerRole.Doctor);

			if (!(Players.Count < 9)) SetPlayerRole(PlayerRole.Chief);

			if (!(Players.Count < 12)) SetPlayerRole(PlayerRole.Lady);

			if (!(Players.Count < 15)) SetPlayerRole(PlayerRole.Psychopath);

			IsRolesGiven = true;
		}));
	}
	private Command _saveCommand;
	public Command SaveCommand
	{
		get => _saveCommand ?? (_saveCommand = new Command(obj =>
		{
			if (_windowModel.TryContinue()) return;

			_playersViewModel.SaveBackup();

			// Saving players in role-based classes
			for(int i = 0; i < Players.Count; i++)
			{
				switch (Players[i].Role)
				{
					case PlayerRole.Godfather:
						Players[i] = new Godfather(Players[i]);
						break;
					case PlayerRole.Doctor:
						Players[i] = new Doctor(Players[i]);
						break;
					case PlayerRole.Lady:
						Players[i] = new Lady(Players[i]);
						break;
					case PlayerRole.Chief:
						Players[i] = new Chief(Players[i]);
						break;
					case PlayerRole.Psychopath:
						Players[i] = new Psychopath(Players[i]);
						break;
				}
			}
			// Moving further
			_windowModel.NextPhase<DayViewModel>(true);
		}));
	}
	int GetRandomNumber(int max)
	{
		var random = new Random();
		return random.Next(max);
	}
	/// <summary>
	/// Sets role to random <see cref="PlayerRole.Civilian"/>
	/// </summary>
	/// <param name="role">Role to set</param>
	void SetPlayerRole(PlayerRole role)
	{
		var selectedPlayers = Players.Where(player => player.Role == PlayerRole.Civilian).ToList();
		var targetPlayer = selectedPlayers[GetRandomNumber(selectedPlayers.Count)];
		targetPlayer.Role = role;
	}

	public override void OnStart()
	{
		SocialMediaProvider.SendToChat("Game starting, you can join using \"/join-game\" command");
	}

	public override void OnEnd()
	{
		SocialMediaProvider.SendToChat("Game started");
	}
}