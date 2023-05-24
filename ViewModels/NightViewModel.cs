using Mafia_panel.Models;
using Mafia_panel.Core;
using System.Collections.ObjectModel;
using Discord;
using System.Linq;
using Mafia_panel.Interfaces;
using Mafia_panel.Models.SocialMedia.Discord;

namespace Mafia_panel.ViewModels;

public class NightViewModel : ViewModelBase
{
	IPlayersViewModel _playersViewModel;
	IGameRulesModel _mode;
	ISocialMediaProvider _socialMediaProvider;
	IMainViewModel _windowModel;

	Player _actorPlayer;
	public Player ActorPlayer
	{
		get => _actorPlayer;
		set => SetValue(ref _actorPlayer, value);
	}
	PlayerRole _actorPlayerRole;
	public PlayerRole ActorPlayerRole
	{
		get => _actorPlayerRole;
		set => SetValue(ref _actorPlayerRole, value);
	}
	Player _targetPlayer;
	public Player TargetPlayer
	{
		get => _targetPlayer;
		set => SetValue(ref _targetPlayer, value);
	}
	private string _actionName = "Kill";
	public string ActionName
	{
		get => _actionName;
		set => SetValue(ref _actionName, value);
	}
	private string _alternativeActionName = "Check";
	public string AlternativeActionName
	{
		get => _alternativeActionName;
		set => SetValue(ref _alternativeActionName, value);
	}
	private bool _isAlternativeActionVisible = false;
	public bool IsAlternativeActionVisible
	{
		get => _isAlternativeActionVisible;
		set => SetValue(ref _isAlternativeActionVisible, value);
	}
	public ObservableCollection<Player> Players => _playersViewModel.Players;

	public NightViewModel(IPlayersViewModel playersViewModel, IGameRulesModel mode, DiscordClientModel discordClient, IMainViewModel windowModel)
	{
		_playersViewModel = playersViewModel;
		_mode = mode;
		_socialMediaProvider = discordClient;
		_windowModel = windowModel;
	}

	/// <summary>
	/// Setups <see cref="ActorPlayer"/> <see cref="ActionName"/> <see cref="TargetPlayer"/> <br/>
	/// and sends possible targets list via <see cref="Player.User"/>
	/// </summary>
	void InitializeTurn()
	{
		ActorPlayer = Players
			.Where(player => player.Role == ActorPlayerRole)
			.First();

		// Setting actions names
		switch (ActorPlayerRole)
		{
			case PlayerRole.Chief:
				ActionName = "Kill";
				IsAlternativeActionVisible = true;
				break;
			case PlayerRole.Doctor:
				ActionName = "Defend";
				IsAlternativeActionVisible = false;
				break;
			case PlayerRole.Lady:
				ActionName = "Stun";
				IsAlternativeActionVisible = false;
				break;
			case PlayerRole.Godfather:
				ActionName = "Kill";
				IsAlternativeActionVisible = _mode.IsGodfatherCanCheck;
				break;
			case PlayerRole.Psychopath:
				ActionName = "Kill";
				IsAlternativeActionVisible = false;
				break;

		}

		TargetPlayer = Players.First();

		//Sending message with possible targets
		string message = "";
		for (int i = 0; i < Players.Count; i++)
		{
			message += $"{i + 1}. {Players[i].Name}" + "\n";
		}
		if (ActorPlayer.User != null)
		{
			ActorPlayer.User.SendMessage("Your Turn, choose target by \"!target <number of target>\" Example:\n!target 3\n" + "Targets:\n" + message);
		}
		ActorPlayer.CanAct = true;
	}
	/// <summary>
	/// Changes <see cref="ActorPlayerRole"/> if it can act this turn
	/// </summary>
	/// <param name="role">Role to set</param>
	public void ChangeTurn(PlayerRole role)
	{
		if ((int)role == 8)
		{
			_windowModel.NextPhase<DayViewModel>();
			return;
		}
		var playerCanAct = Players
			.Where(player => player.Role == role && player.Status != PlayerStatus.StunnedDay && player.Status != PlayerStatus.StunnedNight)
			.Any();

		if(playerCanAct)
		{
			_socialMediaProvider.SendLog($"{role}s Turn");
			ActorPlayerRole = role;
			InitializeTurn();
			return;
		}
		ChangeTurn(role + 1);
	}
	 
	private Command _actionCommand;
	public Command ActionCommand
	{
		get => _actionCommand ??(_actionCommand = new Command(obj =>
		{
			bool canPerform = ActorPlayer.PlayerAction(TargetPlayer, _mode);
			if (!canPerform) return;
			ActorPlayer.CanAct = false;
			ChangeTurn(ActorPlayerRole + 1);
		}));
	}
	private Command _altenativeActionCommand;
	public Command AltenativeActionCommand
	{
		get => _altenativeActionCommand ?? (_altenativeActionCommand = new Command(obj =>
		{
			bool canPerform = ActorPlayer.PlayerAlternativeAction(TargetPlayer, _mode);
			if (!canPerform) return;
			ActorPlayer.CanAct = false;
			ChangeTurn(ActorPlayerRole + 1);
		}));
	}
}
