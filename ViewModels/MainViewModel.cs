using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Mafia_panel.Core;
using Mafia_panel.Models;

namespace Mafia_panel.ViewModels;

class MainViewModel : ViewModelBase
{
	IDiscordClientModel _discordClient;
	PlayersViewModel _playersViewModel;
	ObservableCollection<Player> Players => _playersViewModel.Players;
	GameModeModel _mode;
	ViewModelBase _currentViewModel;
	public ViewModelBase CurrentViewModel
	{
		get => _currentViewModel;
		set => SetProperty(ref _currentViewModel, value);
	}

	Window _window;

	public MainViewModel(PlayersViewModel playersViewModel, GameModeModel mode, IDiscordClientModel discordClient, MainWindow window)
	{
		_playersViewModel = playersViewModel;
		_discordClient = discordClient;
		_mode = mode;
		_window = window;
		CurrentViewModel = new InitialViewModel(_playersViewModel, _mode, InitialSave, InitialConfigure);
	}

	void Inherit()
	{
		var selectedPlayers = new List<int>();
		var seed = new Random();
		var random = new Random(seed.Next());
		foreach (Player player in Players) if (player.Role == PlayerRole.Mafiozo) selectedPlayers.Add(Players.IndexOf(player));
		if (selectedPlayers.Count == 0) return;
		var curatorNum = selectedPlayers[random.Next(selectedPlayers.Count)];
		Players[curatorNum] = new Curator(Players[curatorNum], Inherit);
	}
	void InitialSave()
	{
		_playersViewModel.SaveBackup();
		_discordClient.SendInitialStatus(Players, _mode);
		for (int i = 0; i < Players.Count; i++)
		{
			switch (Players[i].Role)
			{
				case PlayerRole.Godfather:
					Players[i] = new Curator(Players[i], Inherit);
					break;
				case PlayerRole.Doctor:
					Players[i] = new Resuscitator(Players[i]);
					break;
				case PlayerRole.Prostitute:
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
		CurrentViewModel = new DayViewModel(DayEnd, _playersViewModel);
	}
	void InitialConfigure() => _discordClient.ConfigurePlayers(Players);
	bool IsGameOver()
	{
		var badGuys = 0; var goodGuys = 0; var psycho = 0;
		foreach (var player in Players)
		{
			if (player.Role == PlayerRole.Mafiozo || player.Role == PlayerRole.Godfather) badGuys++;
			else if (player.Role == PlayerRole.Psychopath) psycho++;
			else goodGuys++;
		}
		if (badGuys > goodGuys + psycho)
		{
			Win(true);
			return true;
		}
		else if (badGuys + psycho == 0)
		{
			Win(false);
			return true;
		}
		return false;
	}
	void Win(bool isMafiaWins)
	{
		if (isMafiaWins) _discordClient.Send("<@&977568272029478962> Game over, bad guys wins", 915884902401073152);
		else _discordClient.Send("<@&977568272029478962> Game over, good guys wins", 915884902401073152);
		_playersViewModel.LoadBackup();
		_discordClient.SendStatus(Players, 915884902401073152);
		CurrentViewModel = new InitialViewModel(_playersViewModel, _mode, InitialSave, InitialConfigure);
	}
	void DayEnd()
	{
		_discordClient.SendStatus(Players);
		_playersViewModel.ClearKilled();
		if (IsGameOver()) return;
		_playersViewModel.ClearStatus();
		CurrentViewModel = new NightViewModel(NightEnd, _playersViewModel, _mode, _discordClient);
	}
	void NightEnd()
	{
		_discordClient.SendStatus(Players);
		_playersViewModel.ClearKilled();
		if (IsGameOver()) return;
		_playersViewModel.ClearStatus();
		CurrentViewModel = new DayViewModel(DayEnd, _playersViewModel);
	}

	private RelayCommand _minimizeCommand;
	public RelayCommand MinimizeCommand
	{
		get => _minimizeCommand ?? (_minimizeCommand = new RelayCommand(obj =>	_window.WindowState= WindowState.Minimized));
	}

	private RelayCommand _maximizeCommand;
	public RelayCommand MaximizeCommand
	{
		get => _maximizeCommand ?? (_maximizeCommand = new RelayCommand(obj => _window.WindowState ^= WindowState.Maximized));
	}

	private RelayCommand _closeCommand;
	public RelayCommand CloseCommand
	{
		get => _closeCommand ?? (_closeCommand = new RelayCommand(obj => _window.Close()));
	}
}