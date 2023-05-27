using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Mafia_panel.Core;
using Mafia_panel.Models;
using Mafia_panel.Interfaces;
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
	PhaseViewModel CurrentViewModel { get; set; }

	// Commands for window controls
	Command CloseCommand { get; }
	Command MaximizeCommand { get; }
	Command MinimizeCommand { get; }
	/// <summary>
	/// Navigates to settings
	/// </summary>
	Command MenuCommand { get; }

	/// <summary>
	/// Checks if it's time to end the game
	/// </summary>
	bool IsGameOver();
	/// <summary>
	/// Changes <see cref="CurrentViewModel"/>
	/// </summary>
	/// <typeparam name="T">Exact type of ViewModel to switch</typeparam>
	void SwitchCurrentViewModelTo<T>() where T : PhaseViewModel;
	/// <summary>
	/// Clears players and statuses, checks <see cref="IsGameOver"/> and <see cref="SwitchCurrentViewModelTo{T}"/>
	/// </summary>
	/// <typeparam name="T">Exact type of next stage ViewModel</typeparam>
	public void NextPhase<T>(bool isStart = false) where T : PhaseViewModel;
	/// <summary>
	/// Sets <see cref="CurrentViewModel"/> to last ViewModel 
	/// </summary>
	/// <returns>Whether the operation was successful</returns>
	bool TryContinue();
}

public class MainViewModel : NotifyPropertyChanged, IMainViewModel
{
	ISocialMediaProvider _socialMediaProvider;
	IPlayersViewModel _playersViewModel;
	IGameRulesModel _gameRules;
	ObservableCollection<Player> Players => _playersViewModel.Players;
	PhaseViewModel _savedViewModel;
	PhaseViewModel _currentViewModel;
	public PhaseViewModel CurrentViewModel
	{
		get => _currentViewModel;
		set => SetValue(ref _currentViewModel, value);
	}
	private Window _window;
	public Window MainWindow
	{
		get => _window;
		set => SetValue(ref _window, value);
	}

	public MainViewModel(IPlayersViewModel playersViewModel, ISocialMediaProvider socialMediaProvider, IGameRulesModel gameModeModel)
	{
		_playersViewModel = playersViewModel;
		_socialMediaProvider = socialMediaProvider;
		_gameRules = gameModeModel;
	}
	public void SwitchCurrentViewModelTo<T>() where T : PhaseViewModel
	{
		CurrentViewModel = App.Host.Services.GetRequiredService<T>();
	}
	public void NextPhase<T>(bool isStart = false) where T : PhaseViewModel
	{
		string message = "";
		if (isStart)
		{
			message += "New game" + "\n" +
				"Modifications: " + "\n" +
				"Is defense stunning: " + _gameRules.IsDefenseStunning.ToString() + "\n" +
				"Is Godfather Can Check: " + _gameRules.IsGodfatherCanCheck.ToString() + "\n" +
				"Is Chief Limited Kills: " + _gameRules.IsChiefLimitedKills.ToString() + "\n" +
				"Chief Limited Kills: " + _gameRules.ChiefLimitedKills.ToString() + "\n" +
				"Is Chief Cannot Kill Checked: " + _gameRules.IsChiefCannotKillChecked.ToString() + "\n" + "\n";
		}

		message += "Status:" + "\n";
		
		// Getting information for each player
		foreach (Player player in _playersViewModel.Players)
		{
			message += $"{player.Name} - " + player.Role.ToString() + " - " + player.Status.ToString() + "\n";
		}
		_socialMediaProvider.SendLog(message);

		_playersViewModel.ClearKilled();
		_playersViewModel.ClearStatus();

		if(isStart)
		{
			// Send to each player information about their roles
			foreach (var player in _playersViewModel.Players)
			{
				if (player.User != null) player.User.SendMessage(Templates.RoleTemplates[player.Role]);
			}
		}

		if (IsGameOver()) return;

		CurrentViewModel.OnEnd();
		SwitchCurrentViewModelTo<T>();
		CurrentViewModel.OnStart();
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
		_socialMediaProvider.SendToChat($"Game over, {winner}  wins");
		_playersViewModel.LoadBackup();
		_savedViewModel = null;
		NextPhase<SettingsViewModel>();
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

	private Command _minimizeCommand;
	public Command MinimizeCommand
	{
		get => _minimizeCommand ?? (_minimizeCommand = new Command(obj => MainWindow.WindowState = WindowState.Minimized));
	}

	private Command _maximizeCommand;
	public Command MaximizeCommand
	{
		get => _maximizeCommand ?? (_maximizeCommand = new Command(obj => MainWindow.WindowState ^= WindowState.Maximized));
	}

	private Command _closeCommand;
	public Command CloseCommand
	{
		get => _closeCommand ?? (_closeCommand = new Command(obj => MainWindow.Close()));
	}
	private Command _menuCommand;
	public Command MenuCommand
	{
		get => _menuCommand ?? (_menuCommand = new Command(obj => ShowMenu()));
	}
}