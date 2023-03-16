using Mafia_panel.Core;
using Mafia_panel.Models;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mafia_panel.ViewModels;

public interface IPlayersViewModel
{
	/// <summary>
	/// List of active players in the game
	/// </summary>
	ObservableCollection<Player> Players { get; set; }

	/// <summary>
	/// Clears killed players from <see cref="Players"/>
	/// </summary>
	void ClearKilled();
	/// <summary>
	/// Cycling Stun and clears statuses from all players
	/// </summary>
	void ClearStatus();
	/// <summary>
	/// Saves <see cref="Players"/>
	/// </summary>
	void SaveBackup();
	/// <summary>
	/// Loads <see cref="Players"/>
	/// </summary>
	void LoadBackup();
}

public class PlayersViewModel : ViewModelBase, IPlayersViewModel
{
	ObservableCollection<Player> _players;
	IGameModeModel _gameRules;
	public ObservableCollection<Player> Players
	{
		get => _players;
		set => SetProperty(ref _players, value);
	}
	ObservableCollection<Player> _playersBackup;
	public PlayersViewModel(IGameModeModel gameRules)
	{
		_gameRules = gameRules;
		_players = new ObservableCollection<Player>();
		_playersBackup = new ObservableCollection<Player>();
	}
	public void ClearKilled()
	{
		Players.Where(player => player.Status == PlayerStatus.Killed)
			.ToList()
			.All(Players.Remove);
		if (Players.Where(player => player.Role == PlayerRole.Godfather).Count() == 0)
		{
			GodfatherInherit();
		}
	}
	public void ClearStatus()
	{
		foreach (var player in Players)
		{
			if ( (player.Status == PlayerStatus.StunnedNight) || (player.Status == PlayerStatus.Defended && _gameRules.IsDefenseStunning) )
			{
				player.Status = PlayerStatus.StunnedDay;
			}
			else
			{
				player.Status = PlayerStatus.None;
			}
		}
	}
	void GodfatherInherit()
	{
		List<Player> selectedPlayers = new List<Player>(Players.Where(player => player.Role == PlayerRole.Mafioso));

		if (selectedPlayers.Count == 0) return;

		var newGodfather = selectedPlayers[Random.Shared.Next(selectedPlayers.Count)];

		Players[Players.IndexOf(newGodfather)] = new Godfather(newGodfather);
	}
	public void SaveBackup() => _playersBackup = new ObservableCollection<Player>(Players);
	public void LoadBackup() => Players = new ObservableCollection<Player>(_playersBackup);
}
