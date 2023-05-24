using Mafia_panel.Models;
using Mafia_panel.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mafia_panel.ViewModels;

internal class DayViewModel : ViewModelBase
{
	List<Player> _maxVotedPlayers; 
	IPlayersViewModel _playersViewModel;
	IMainViewModel _windowModel;
	public ObservableCollection<Player> Players => _playersViewModel.Players;
	Player _selectedPlayer;
	public Player SelectedPlayer
	{
		get => _selectedPlayer ?? (_selectedPlayer = Players.FirstOrDefault());
		set => SetValue(ref _selectedPlayer, value);
	}
	public string _targetName = "Vote";
	public string TargetName
	{
		get => _targetName;
		set => SetValue(ref _targetName, value);
	}
	public bool _canProceed = false;
	public bool CanProceed
	{
		get => _canProceed;
		set => SetValue(ref _canProceed, value);
	}
	public DayViewModel(IPlayersViewModel playersViewModel, IMainViewModel windowModel)
	{
		_playersViewModel = playersViewModel;
		_windowModel = windowModel;
	}

	/// <summary>
	/// Searches for players with max number of votes
	/// </summary>
	void GetMaxVoted()
	{
		// Searching players with most votes
		var maxvotes = Players.Max(player => player.Votes);
		_maxVotedPlayers = Players.Where(player => player.Votes == maxvotes).ToList();

		// Selecting name of most voted player if able
		switch(_maxVotedPlayers.Count)
		{
			case 0:
				CanProceed = false;
				TargetName = "Vote";
				break;
			case 1:
				CanProceed = true;
				TargetName = _maxVotedPlayers[0].Name + " will be killed";
				break;
			default:
				CanProceed = true;
				TargetName = "Nobody" + " will be killed";
				break;
		}
	}

	private Command _addVoteCommand;
	public Command AddVoteCommand
	{
		get => _addVoteCommand ?? (_addVoteCommand = new Command(obj => 
		{
			SelectedPlayer.Votes++;
			GetMaxVoted();
		}));
	}
	private Command _removeVoteCommand;
	public Command RemoveVoteCommand
	{
		get => _removeVoteCommand ??(_removeVoteCommand = new Command(obj =>
		{
			SelectedPlayer.Votes--;
			GetMaxVoted();
		}));
	}
	private Command _voteCommand;
	public Command VoteCommand
	{
		get => _voteCommand ?? (_voteCommand = new Command(obj =>
		{
			if (_maxVotedPlayers.Count == 1)
			{
				_maxVotedPlayers.First().Kill();
			}
			Players.ToList().ForEach(player => player.CanVote = false);
			_windowModel.NextPhase<NightViewModel>();
			CanProceed = false;
		}));
	}
}
