using Mafia_panel.Core;
using Mafia_panel.Models;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mafia_panel.ViewModels;

public interface IPlayersViewModel
{
	ObservableCollection<Player> Players { get; set; }

	void ClearKilled();
	void ClearStatus();
	void LoadBackup();
	void SaveBackup();
}

public class PlayersViewModel : ViewModelBase, IPlayersViewModel
{
	ObservableCollection<Player> _players;
	public ObservableCollection<Player> Players
	{
		get => _players;
		set => SetProperty(ref _players, value);
	}
	ObservableCollection<Player> _playersBackup;
	public PlayersViewModel()
	{
		_players = new ObservableCollection<Player>();
		_playersBackup = new ObservableCollection<Player>();
	}
	public void ClearKilled()
	{
		Players.Where(player => player.Status == PlayerStatus.Killed).ToList().All(Players.Remove);
		if (Players.Where(player => player.Role == PlayerRole.Godfather).Count() == 0)
		{
			GodfatherInherit();
		}
	}
	public void ClearStatus()
	{
		foreach (var player in Players)
		{
			if (player.Status == PlayerStatus.Stunned0)
			{
				player.Status = PlayerStatus.Stunned1;
			}
			else player.Status = PlayerStatus.None;
		}
	}
	void GodfatherInherit()
	{
		List<Player> selectedPlayers = new List<Player>(Players.Where(player => player.Role == PlayerRole.Mafiozo));

		if (selectedPlayers.Count == 0) return;

		var newGodfather = selectedPlayers[Random.Shared.Next(selectedPlayers.Count)];

		Players[Players.IndexOf(newGodfather)] = new Godfather(newGodfather);
	}
	public void SaveBackup() => _playersBackup = new ObservableCollection<Player>(Players);
	public void LoadBackup() => Players = new ObservableCollection<Player>(_playersBackup);
}
