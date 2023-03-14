using Mafia_panel.Models;
using Mafia_panel.Core;
using System.Windows;
using System.Collections.ObjectModel;
using Discord;

namespace Mafia_panel.ViewModels;

class NightViewModel : ViewModelBase
{
	IPlayersViewModel _playersViewModel;
	IGameModeModel _mode;
	IDiscordClientModel _discordClient;
	IMainViewModel _windowModel;

	Player _actorPlayer;
	public Player ActorPlayer
	{
		get => _actorPlayer;
		set => SetProperty(ref _actorPlayer, value);
	}
	PlayerRole _actorPlayerRole;
	public PlayerRole ActorPlayerRole
	{
		get => _actorPlayerRole;
		set => SetProperty(ref _actorPlayerRole, value);
	}
	Player _targetPlayer;
	public Player TargetPlayer
	{
		get => _targetPlayer;
		set => SetProperty(ref _targetPlayer, value);
	}
	private string _actionName = "Kill";
	public string ActionName
	{
		get => _actionName;
		set => SetProperty(ref _actionName, value);
	}
	private string _alternativeActionName = "Check";
	public string AlternativeActionName
	{
		get => _alternativeActionName;
		set => SetProperty(ref _alternativeActionName, value);
	}
	private bool _isAlternativeActionVisible = false;
	public bool IsAlternativeActionVisible
	{
		get => _isAlternativeActionVisible;
		set => SetProperty(ref _isAlternativeActionVisible, value);
	}
	public ObservableCollection<Player> Players => _playersViewModel.Players;

	public NightViewModel(IPlayersViewModel playersViewModel, IGameModeModel mode, IDiscordClientModel discordClient, IMainViewModel windowModel)
	{
		_playersViewModel = playersViewModel;
		_mode = mode;
		_discordClient = discordClient;
		_windowModel = windowModel;
	}
	void InitializeTurn()
	{
		foreach (var player in Players) 
			if (player.Role == ActorPlayerRole) 
				ActorPlayer = player;

		switch (ActorPlayerRole)
		{
			case PlayerRole.Chief:
				ActionName = "Kill";
				IsAlternativeActionVisible = true;
				break;
			case PlayerRole.Doctor:
				ActionName = "Defend";
				break;
			case PlayerRole.Anesthesiologist:
				ActionName = "Stun";
				break;
			case PlayerRole.Godfather:
				ActionName = "Kill";
				IsAlternativeActionVisible = _mode.IsGodfatherCanCheck;
				break;
		}

		TargetPlayer = Players[0];
		string message = "";
		for (int i = 0; i < Players.Count; i++) message += $"{i + 1}. {Players[i].Name}" + "\n";
		ActorPlayer.User.SendMessageAsync("Your Turn, choose target by \"!target <number of target>\" Example:\n!target 3\n" + "Targets:\n" + message);
	}
	public void ChangeTurn(PlayerRole role)
	{
		if ((int)role == 8)
		{
			_windowModel.NextPhase<DayViewModel>();
			return;
		}
		var playerCanAct = false;
		foreach (var player in _playersViewModel.Players) 
			if (player.Role == role && player.Status != PlayerStatus.Stunned1) 
				playerCanAct = true;
		if(playerCanAct)
		{
			_discordClient.SendToLogChannel($"{role}s Turn");
			ActorPlayerRole = role;
			InitializeTurn();
			return;
		}
		ChangeTurn(role + 1);
	}
	 
	private RelayCommand _actionCommand;
	public RelayCommand ActionCommand
	{
		get
		{
			return _actionCommand ??
				(_actionCommand = new RelayCommand(obj =>
				{
					if (MessageBox.Show($"{TargetPlayer.Name} will be {ActionName}ed \n" +
							"Do you want to continue?",
							"Next Turn",
							MessageBoxButton.YesNo,
							MessageBoxImage.Question) == MessageBoxResult.No) return;
					bool canPerform = ActorPlayer.PlayerAction(TargetPlayer, _mode);
					if (!canPerform) return;
					ChangeTurn(ActorPlayerRole + 1);
				}));
		}
	}
	private RelayCommand _altenativeActionCommand;
	public RelayCommand AltenativeActionCommand
	{
		get
		{
			return _altenativeActionCommand ??
				(_altenativeActionCommand = new RelayCommand(obj =>
				{
					if (MessageBox.Show($"{TargetPlayer.Name} will be {AlternativeActionName}ed \n" +
							"Do you want to continue?",
							"Next Turn",
							MessageBoxButton.YesNo,
							MessageBoxImage.Question) == MessageBoxResult.No) return;
					bool canPerform = ActorPlayer.PlayerAlternativeAction(TargetPlayer, _mode);
					if (!canPerform) return;
					ChangeTurn(ActorPlayerRole + 1);
				}));
		}
	}
}
