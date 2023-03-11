using System.Collections.ObjectModel;
using System.Windows;
using Mafia_panel.Core;
using Mafia_panel.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Mafia_panel.ViewModels;

public class MainViewModel : ViewModelBase
{
	IDiscordClientModel _discordClient;
	IPlayersViewModel _playersViewModel;
	ObservableCollection<Player> Players => _playersViewModel.Players;
	ViewModelBase _savedViewModel;
	ViewModelBase _currentViewModel;
	public ViewModelBase CurrentViewModel
	{
		get => _currentViewModel;
		set => SetProperty(ref _currentViewModel, value);
	}
	public Window Window;

	public MainViewModel(IPlayersViewModel playersViewModel, IDiscordClientModel discordClient)
	{
		_playersViewModel = playersViewModel;
		_discordClient = discordClient;
	}
	public void SwitchCurrentViewModelTo<T>() where T : ViewModelBase
	{
		CurrentViewModel = App.Host.Services.GetRequiredService<T>();
	}
	public bool IsGameOver()
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
		_savedViewModel = null;
		SwitchCurrentViewModelTo<InitialViewModel>();
	}
	public void NightEnd()
	{
		_discordClient.SendStatus(Players);
		_playersViewModel.ClearKilled();
		if (IsGameOver()) return;
		_playersViewModel.ClearStatus();
		SwitchCurrentViewModelTo<DayViewModel>();
	}
	void ShowMenu()
	{
		_savedViewModel = CurrentViewModel;
		SwitchCurrentViewModelTo<InitialViewModel>();
	}
	public bool TryContinue() 
	{ 
		if(_savedViewModel == null) return false;
		CurrentViewModel = _savedViewModel;
		return true;
	} 

	private RelayCommand _minimizeCommand;
	public RelayCommand MinimizeCommand
	{
		get => _minimizeCommand ?? (_minimizeCommand = new RelayCommand(obj => Window.WindowState = WindowState.Minimized));
	}

	private RelayCommand _maximizeCommand;
	public RelayCommand MaximizeCommand
	{
		get => _maximizeCommand ?? (_maximizeCommand = new RelayCommand(obj => Window.WindowState ^= WindowState.Maximized));
	}

	private RelayCommand _closeCommand;
	public RelayCommand CloseCommand
	{
		get => _closeCommand ?? (_closeCommand = new RelayCommand(obj => Window.Close()));
	}
	private RelayCommand _menuCommand;
	public RelayCommand MenuCommand
	{
		get => _menuCommand ?? (_menuCommand = new RelayCommand(obj => ShowMenu()));
	}
}