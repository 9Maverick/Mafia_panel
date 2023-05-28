using Mafia_panel.Models;
using Mafia_panel.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using Mafia_panel.Interfaces;

namespace Mafia_panel.ViewModels;

internal class DayViewModel : PhaseViewModel
{
	List<Player> _maxVotedPlayers; 
	IPlayersViewModel _playersViewModel;
	MainViewModel _windowModel;
	ISocialMediaProvider _socialMediaProvider;
	public ObservableCollection<Player> Players => _playersViewModel.ActivePlayers;
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
	public DayViewModel(IPlayersViewModel playersViewModel, MainViewModel windowModel, ISocialMediaProvider socialMediaProvider)
	{
		_playersViewModel = playersViewModel;
		_windowModel = windowModel;
		_socialMediaProvider = socialMediaProvider;
		_maxVotedPlayers = new List<Player>();
	}

	/// <summary>
	/// Searches for players with max number of votes
	/// </summary>
	void GetMaxVoted()
	{
		// Searching players with most votes
		var maxvotes = Players.Max(player => player.Votes);
		_maxVotedPlayers = Players.Where(player => player.Votes == maxvotes).ToList();

		ChangeVoteState();
	}
	/// <summary>
	/// Changes state of voting process depending of number of voted players with maximum amount of votes
	/// </summary>
	void ChangeVoteState()
	{
		if (_maxVotedPlayers.Count == 1)
		{
			TargetName = _maxVotedPlayers[0].Name + " will be executed";
			return;
		}
		TargetName = "Nobody" + " will be executed";
	}

	public override void OnStart()
	{
		_playersViewModel.ActivePlayers.ToList().ForEach(player => 
		{
			player.CanVote = true;
			player.PropertyChanged += OnVotesChanged;
		});
		//Sending message with possible vote targets
		string message = "";
		for (int i = 0; i < Players.Count; i++)
		{
			message += $"{i + 1}. {Players[i].Name}" + "\n";
		}
		_socialMediaProvider.SendToChat("Time to vote, choose target by \"/vote <number of target>\"\n" +
				"Example: /vote 3\n" +
				"Players:\n" +
				message);
	}

	public override void OnEnd()
	{
		_maxVotedPlayers.Clear();
		_playersViewModel.ActivePlayers.ToList().ForEach(player =>
		{
			player.CanVote = false;
			player.PropertyChanged -= OnVotesChanged;
		});
		TargetName = "Nobody" + " will be executed";
	}
	void OnVotesChanged(object? sender, PropertyChangedEventArgs args)
	{
		if (!(args.PropertyName == nameof(Player.Votes))) return;
		GetMaxVoted();
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
			if (SelectedPlayer.Votes == 0) return;
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
			_windowModel.NextPhase<NightViewModel>();
		}));
	}
}
