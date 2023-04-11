using Mafia_panel.Models;
using Mafia_panel.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mafia_panel.ViewModels;

internal class SettingsViewModel : ViewModelBase
{
	IPlayersViewModel _playersViewModel;
	IMainViewModel _windowModel;

	IDiscordClientModel _discordClient;
	public IDiscordClientModel DiscordClientModel
	{
		get => _discordClient;
		set => SetProperty(ref _discordClient, value);
	}
	private Player _selectedPlayer;
	public Player SelectedPlayer
	{
		get => _selectedPlayer;
		set => SetProperty(ref _selectedPlayer, value);
	}
	public ObservableCollection<Player> Players => _playersViewModel.Players;

	IGameRulesModel _mode;
	public IGameRulesModel Mode
	{
		get => _mode;
		set => SetProperty(ref _mode, value);
	}
	private bool _isRolesGiven = false;
	public bool IsRolesGiven
	{
		get => _isRolesGiven;
		set => SetProperty(ref _isRolesGiven, value);
	}
	private bool _isDiscordOn = false;
	public bool IsDiscordOn
	{
		get => _isDiscordOn;
		set => SetProperty(ref _isDiscordOn, value);
	}

	public SettingsViewModel(IPlayersViewModel playersViewModel, IGameRulesModel mode, IDiscordClientModel discordClient, IMainViewModel windowModel)
	{
		_playersViewModel = playersViewModel;
		_mode = mode;
		_discordClient = discordClient;
		_windowModel = windowModel;
	}

	private RelayCommand _addPlayerCommand;
	public RelayCommand AddPlayerCommand
	{
		get => _addPlayerCommand ??	(_addPlayerCommand = new RelayCommand(obj =>
		{
			Players.Add(new Player());
			IsRolesGiven = false;
		}));
	}
	private RelayCommand _removePlayerCommand;
	public RelayCommand RemovePlayerCommand
	{
		get => _removePlayerCommand ?? (_removePlayerCommand = new RelayCommand(obj => 
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

	private RelayCommand _addRolesCommand;
	/// <summary>
	/// Add <see cref="PlayerRole"/> to each <see cref="Player"/>
	/// </summary>
	public RelayCommand AddRolesCommand
	{
		get => _addRolesCommand ??(_addRolesCommand = new RelayCommand(obj =>
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
	private RelayCommand _saveCommand;
	public RelayCommand SaveCommand
	{
		get => _saveCommand ?? (_saveCommand = new RelayCommand(obj =>
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
			_windowModel.NextPhase<DayViewModel>();
			_discordClient.SendInitialStatus();
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
	
}