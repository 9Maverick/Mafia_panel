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
	MainViewModel _windowModel;

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
	public ObservableCollection<Player> ActivePlayers => _playersViewModel.ActivePlayers;

	IGameRulesModel _mode;
	public IGameRulesModel Mode
	{
		get => _mode;
		set => SetValue(ref _mode, value);
	}
	private bool _canStart = false;
	public bool CanStart
	{
		get => _canStart;
		set => SetValue(ref _canStart, value);
	}
	private bool _canContinue = false;
	public bool CanContinue
	{
		get => _canContinue;
		set => SetValue(ref _canContinue, value);
	}

	public SettingsViewModel(IPlayersViewModel playersViewModel, IGameRulesModel mode, SocialMediaProvider socialMediaProvider, MainViewModel windowModel)
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
			Players.Add(new Player("Player " + (Players.Count + 1)));
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

			CanStart = true;
		}));
	}
	private Command _saveCommand;
	public Command SaveCommand
	{
		get => _saveCommand ?? (_saveCommand = new Command(obj =>
		{
			_playersViewModel.LoadPlayers();

			// Saving players in role-based classes
			for(int i = 0; i < ActivePlayers.Count; i++)
			{
				switch (ActivePlayers[i].Role)
				{
					case PlayerRole.None:
						ActivePlayers.Remove(ActivePlayers[i]);
						break;
					case PlayerRole.Godfather:
						ActivePlayers[i] = new Godfather(ActivePlayers[i]);
						break;
					case PlayerRole.Doctor:
						ActivePlayers[i] = new Doctor(ActivePlayers[i]);
						break;
					case PlayerRole.Lady:
						ActivePlayers[i] = new Lady(ActivePlayers[i]);
						break;
					case PlayerRole.Chief:
						ActivePlayers[i] = new Chief(ActivePlayers[i]);
						break;
					case PlayerRole.Psychopath:
						ActivePlayers[i] = new Psychopath(ActivePlayers[i]);
						break;
				}
			}
			// Moving further
			_windowModel.NextPhase<DayViewModel>(true);
		}));
	}
	Command _continueCommand;
	public Command ContinueCommand
	{
		get => _continueCommand ?? (_continueCommand = new Command(obj => _windowModel.TryContinue()));
	}

	private Command _notifyCommand;
	public Command NotifyCommand
	{
		get => _notifyCommand ?? (_notifyCommand = new Command(obj =>
		{
			SocialMediaProvider.SendToChat("Game starting, you can join using \"/joingame\" command");
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
	void UpdateRolesGiven(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => CanStart = false;

	public override void OnStart()
	{
		_playersViewModel.Players.CollectionChanged += UpdateRolesGiven;
		// Check if game can be continued
		CanContinue = _windowModel.SavedViewModel != null;
		CanStart = CanStart && !Players.Where(player => player.Role == PlayerRole.None).Any();
	}

	public override void OnEnd()
	{
	}
}