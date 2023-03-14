using Mafia_panel.Models;
using Mafia_panel.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mafia_panel.ViewModels;

internal class InitialViewModel : ViewModelBase
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

	IGameModeModel _mode;
	public IGameModeModel Mode
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

	public InitialViewModel(IPlayersViewModel playersViewModel, IGameModeModel mode, IDiscordClientModel discordClient, IMainViewModel windowModel)
	{
		_playersViewModel = playersViewModel;
		_mode = mode;
		_discordClient = discordClient;
		_windowModel = windowModel;
	}

	private RelayCommand _addPlayerCommand;
	public RelayCommand AddPlayerCommand
	{
		get => _addPlayerCommand ??	(_addPlayerCommand = new RelayCommand(obj => Players.Add(new Player())));
	}
	private RelayCommand _removePlayerCommand;
	public RelayCommand RemovePlayerCommand
	{
		get => _removePlayerCommand ?? (_removePlayerCommand = new RelayCommand(obj => Players.Remove(SelectedPlayer)));
	}

	private RelayCommand _addRolesCommand;
	public RelayCommand AddRolesCommand
	{
		get
		{
			return _addRolesCommand ??
				(_addRolesCommand = new RelayCommand(obj =>
				{
					if (Players.Count < 3) return;
					foreach (Player player in Players) player.Role = PlayerRole.Civilian;

					var selectedPlayers = new List<int>();
					int vivisectorCount = Players.Count / 3;
					for (int i = 0; i < vivisectorCount; i++)
					{
						SetPlayerRole(PlayerRole.Mafiozo);
					}
					foreach (Player player in Players) if (player.Role == PlayerRole.Mafiozo) selectedPlayers.Add(Players.IndexOf(player));
					var curatorNum = selectedPlayers[GetRandomNumber(selectedPlayers.Count)];
					Players[curatorNum].Role = PlayerRole.Godfather;

					
					if (!(Players.Count < 6))   SetPlayerRole(PlayerRole.Doctor);

					if (!(Players.Count < 9))   SetPlayerRole(PlayerRole.Chief);

					if (!(Players.Count < 12))  SetPlayerRole(PlayerRole.Anesthesiologist);

					if (!(Players.Count < 15))  SetPlayerRole(PlayerRole.Psychopath);

					IsRolesGiven = true;
				}));
		}
	}
	private RelayCommand _saveCommand;
	public RelayCommand SaveCommand
	{
		get => _saveCommand ?? (_saveCommand = new RelayCommand(obj =>
		{
			if (_windowModel.TryContinue()) return;

			_playersViewModel.SaveBackup();
			_discordClient.SendInitialStatus(Players, _mode);
			for (int i = 0; i < Players.Count; i++)
			{
				switch (Players[i].Role)
				{
					case PlayerRole.Godfather:
						Players[i] = new Godfather(Players[i]);
						break;
					case PlayerRole.Doctor:
						Players[i] = new Doctor(Players[i]);
						break;
					case PlayerRole.Anesthesiologist:
						Players[i] = new Anesthesiologist(Players[i]);
						break;
					case PlayerRole.Chief:
						Players[i] = new Chief(Players[i]);
						break;
					case PlayerRole.Psychopath:
						Players[i] = new Psychopath(Players[i]);
						break;
				}
			}
			_windowModel.SwitchCurrentViewModelTo<DayViewModel>();
		}));
	}
	int GetRandomNumber(int max)
	{
		var random = new Random();
		return random.Next(max);
	}
	void SetPlayerRole(PlayerRole role)
	{
		var selectedPlayers = new List<int>();
		foreach (Player player in Players) if (player.Role == PlayerRole.Civilian) selectedPlayers.Add(Players.IndexOf(player));
		var roleNum = selectedPlayers[GetRandomNumber(selectedPlayers.Count)];
		Players[roleNum].Role = role;
	}
	
}