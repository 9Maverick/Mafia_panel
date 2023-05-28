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
	ObservableCollection<Player> ActivePlayers { get; set; }
	/// <summary>
	/// List of all players
	/// </summary>
	ObservableCollection<Player> Players { get; set; }

	/// <summary>
	/// Clears killed players from <see cref="ActivePlayers"/>
	/// </summary>
	void ClearKilled();
	/// <summary>
	/// Cycling Stun and clears statuses from all players
	/// </summary>
	void ClearStatus();
	/// <summary>
	/// Loads <see cref="ActivePlayers"/>
	/// </summary>
	void LoadPlayers();
	/// <summary>
	/// Searches players from <see cref="Players"/>
	/// </summary>
	/// <param name="id"></param>
	/// <returns> <see cref="Player"/> with specified id</returns>
	Player? GetPlayerByUserId(long id);
	/// <summary>
	/// Searches players from <see cref="ActivePlayers"/>
	/// </summary>
	/// <param name="id"></param>
	/// <returns> <see cref="Player"/> with specified id</returns>
	Player? GetActivePlayerByUserId(long id);
}

public class PlayersViewModel : NotifyPropertyChanged, IPlayersViewModel
{
	ObservableCollection<Player> _activePlayers;
	IGameRulesModel _gameRules;
	public ObservableCollection<Player> ActivePlayers
	{
		get => _activePlayers;
		set => SetValue(ref _activePlayers, value);
	}
	ObservableCollection<Player> _players;
	public ObservableCollection<Player> Players
	{
		get => _players;
		set => SetValue(ref _players, value);
	}
	public PlayersViewModel(IGameRulesModel gameRules)
	{
		_gameRules = gameRules;
		_activePlayers = new ObservableCollection<Player>();
		_players = new ObservableCollection<Player>();
	}
	public void ClearKilled()
	{
		 ActivePlayers.Where(player => player.Status == PlayerStatus.Killed)
			.ToList()
			.All(ActivePlayers.Remove);
		if (ActivePlayers.Where(player => player.Role == PlayerRole.Godfather).Count() == 0 
			&& ActivePlayers.Where(player => player.Role == PlayerRole.Mafioso).Any())
		{
			GodfatherInherit();
		}
	}
	public void ClearStatus()
	{
		foreach (var player in ActivePlayers)
		{
			if ( (player.Status == PlayerStatus.StunnedNight) || (player.Status == PlayerStatus.Defended && _gameRules.IsDefenseStunning) )
			{
				player.Status = PlayerStatus.StunnedDay;
			}
			else
			{
				player.Status = PlayerStatus.None;
			}
			player.Votes = 0;
		}
	}
	void GodfatherInherit()
	{
		List<Player> selectedPlayers = new List<Player>(ActivePlayers.Where(player => player.Role == PlayerRole.Mafioso));

		if (selectedPlayers.Count == 0) return;

		var newGodfather = selectedPlayers[Random.Shared.Next(selectedPlayers.Count)];

		ActivePlayers[ActivePlayers.IndexOf(newGodfather)] = new Godfather(newGodfather);
	}
	public void LoadPlayers()
	{
		ActivePlayers.Clear();
		Players.ToList().ForEach(ActivePlayers.Add);
		ClearStatus();
	}
	public Player? GetPlayerByUserId(long id) => Players.FirstOrDefault(player => player.User != null && player.User.Id == id);
	public Player? GetActivePlayerByUserId(long id) => ActivePlayers.FirstOrDefault(player => player.User != null && player.User.Id == id);
}
