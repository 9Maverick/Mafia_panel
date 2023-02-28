using Mafia_panel.Core;
using Mafia_panel.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mafia_panel.ViewModels;

internal class PlayersViewModel : ViewModelBase
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
		_players= new ObservableCollection<Player>();
		_playersBackup= new ObservableCollection<Player>();
	}
	public void ClearKilled()
	{
		foreach (var player in Players.ToList())
		{
			if (player.Status == PlayerStatus.Killed)
			{
				Players.Remove(player);
			}
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
	public void SaveBackup() => _playersBackup = new ObservableCollection<Player>(Players);
	public void LoadBackup() => Players = new ObservableCollection<Player>(_playersBackup);
}
