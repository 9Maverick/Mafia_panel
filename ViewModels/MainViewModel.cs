using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Mafia_panel.Core;
using Mafia_panel.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Mafia_panel.ViewModels;

public interface IMainViewModel
{
	/// <summary>
	/// Instance of Application window
	/// </summary>
	Window MainWindow { get; set; }
	/// <summary>
	/// ViewModel of content on window
	/// </summary>
	ViewModelBase CurrentViewModel { get; set; }

	// Commands for window controls
	RelayCommand CloseCommand { get; }
	RelayCommand MaximizeCommand { get; }
	RelayCommand MinimizeCommand { get; }
	/// <summary>
	/// Navigates to settings
	/// </summary>
	RelayCommand MenuCommand { get; }

	/// <summary>
	/// Checks if it's time to end the game
	/// </summary>
	bool IsGameOver();
	/// <summary>
	/// Changes <see cref="CurrentViewModel"/>
	/// </summary>
	/// <typeparam name="T">Exact type of ViewModel to switch</typeparam>
	void SwitchCurrentViewModelTo<T>() where T : ViewModelBase;
	/// <summary>
	/// Clears players and statuses, checks <see cref="IsGameOver"/> and <see cref="SwitchCurrentViewModelTo{T}"/>
	/// </summary>
	/// <typeparam name="T">Exact type of next stage ViewModel</typeparam>
	public void NextPhase<T>() where T : ViewModelBase;
	/// <summary>
	/// Sets <see cref="CurrentViewModel"/> to last ViewModel 
	/// </summary>
	/// <returns>Whether the operation was successful</returns>
	bool TryContinue();
}

public class MainViewModel : ViewModelBase, IMainViewModel
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
	private Window _window;
	public Window MainWindow
	{
		get => _window;
		set => SetProperty(ref _window, value);
	}

	public MainViewModel(IPlayersViewModel playersViewModel, IDiscordClientModel discordClient)
	{
		_playersViewModel = playersViewModel;
		_discordClient = discordClient;
	}
	public void SwitchCurrentViewModelTo<T>() where T : ViewModelBase
	{
		CurrentViewModel = App.Host.Services.GetRequiredService<T>();
	}
	public void NextPhase<T>() where T : ViewModelBase
	{
		_discordClient.SendStatus();

		_playersViewModel.ClearKilled();
		_playersViewModel.ClearStatus();

		if (IsGameOver()) return;

		SwitchCurrentViewModelTo<T>();
		if(CurrentViewModel is NightViewModel)
		{
			var night = CurrentViewModel as NightViewModel;
			night.ChangeTurn(PlayerRole.Godfather);
		}
	}
	public bool IsGameOver()
	{
		// Geting number of players in each side
		var badGuys = Players
			.Where(player => player.Role == PlayerRole.Mafioso || player.Role == PlayerRole.Godfather)
			.Count();
		var goodGuys = Players
			.Where(player => player.Role == PlayerRole.Chief || player.Role == PlayerRole.Civilian || player.Role == PlayerRole.Doctor)
			.Count();
		var psycho = Players
			.Where(player => player.Role == PlayerRole.Psychopath)
			.Count();

		if (badGuys > goodGuys + psycho)
		{
			Win("Mafia");
			return true;
		}
		else if (badGuys + psycho == 0)
		{
			Win("good guys");
			return true;
		}
		else if (psycho > goodGuys)
		{
			Win("Psycho");
			return true;
		}
		return false;
	}
	void Win(string winner)
	{
		_discordClient.SendToAnnounceChannel($"Game over, {winner}  wins");
		_playersViewModel.LoadBackup();
		_savedViewModel = null;
		SwitchCurrentViewModelTo<SettingsViewModel>();
	}
	/// <summary>
	/// Saves current game stages and navigates to <see cref="SettingsViewModel"/>
	/// </summary>
	void ShowMenu()
	{
		if(CurrentViewModel is SettingsViewModel)
		{
			TryContinue();
			return;
		}
		_savedViewModel = CurrentViewModel;
		SwitchCurrentViewModelTo<SettingsViewModel>();
	}
	public bool TryContinue()
	{
		if (_savedViewModel == null) return false;
		CurrentViewModel = _savedViewModel;
		return true;
	}

	private RelayCommand _minimizeCommand;
	public RelayCommand MinimizeCommand
	{
		get => _minimizeCommand ?? (_minimizeCommand = new RelayCommand(obj => MainWindow.WindowState = WindowState.Minimized));
	}

	private RelayCommand _maximizeCommand;
	public RelayCommand MaximizeCommand
	{
		get => _maximizeCommand ?? (_maximizeCommand = new RelayCommand(obj => MainWindow.WindowState ^= WindowState.Maximized));
	}

	private RelayCommand _closeCommand;
	public RelayCommand CloseCommand
	{
		get => _closeCommand ?? (_closeCommand = new RelayCommand(obj => MainWindow.Close()));
	}
	private RelayCommand _menuCommand;
	public RelayCommand MenuCommand
	{
		get => _menuCommand ?? (_menuCommand = new RelayCommand(obj => ShowMenu()));
	}
}