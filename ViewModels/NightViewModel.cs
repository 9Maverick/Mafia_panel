using Mafia_panel.Models;
using Mafia_panel.Core;
using System;
using System.Collections.ObjectModel;


namespace Mafia_panel.ViewModels;

class NightViewModel : ViewModelBase
{
	Action _nightEnd;
	IDiscordClientModel _discordClient;
	PlayersViewModel _playersViewModel;
	GameModeModel _mode;
	TurnViewModel _turn;
	public TurnViewModel Turn
	{
		get => _turn;
		set => SetProperty(ref _turn, value);
	}

	public NightViewModel(Action nightEnd, PlayersViewModel playersViewModel, GameModeModel mode, IDiscordClientModel discordClient)
	{
		_nightEnd = nightEnd;
		_playersViewModel = playersViewModel;
		_mode = mode;
		_discordClient = discordClient;
		ChangeTurn(PlayerRole.Godfather);
	}
	void ChangeTurn(PlayerRole role)
	{
		if ((int)role == 8)
		{
			_nightEnd.Invoke();
			return;
		}
		var playerExist = false;
		foreach (var player in _playersViewModel.Players) if (player.Role == role && player.Status != PlayerStatus.Stunned1) playerExist = true;
		if(playerExist)
		{
			_discordClient.Send($"{role}s Turn");
			Turn = new TurnViewModel(_playersViewModel, _mode, _discordClient, role, () => ChangeTurn(role + 1));
			return;
		}
		ChangeTurn(role + 1);
	}
}
