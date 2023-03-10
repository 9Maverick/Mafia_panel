using Mafia_panel.Models;
using Mafia_panel.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;

namespace Mafia_panel.ViewModels;

internal class DayViewModel : ViewModelBase
{
	IPlayersViewModel _playersViewModel;
	IDiscordClientModel _discordClient;
	MainViewModel _windowModel;
	public ObservableCollection<Player> Players => _playersViewModel.Players;
	Player _selectedPlayer;
	public Player SelectedPlayer
	{
		get => _selectedPlayer ?? (_selectedPlayer = Players.FirstOrDefault());
		set => SetProperty(ref _selectedPlayer, value);
	}
	private bool _isSecondTour = false;
	public bool IsSecondTour
	{
		get => _isSecondTour;
		set => SetProperty(ref _isSecondTour, value);
	}

	public DayViewModel(IPlayersViewModel playersViewModel, IDiscordClientModel discordClient, MainViewModel windowModel)
	{
		_playersViewModel = playersViewModel;
		_discordClient = discordClient;
		_windowModel = windowModel;
	}

	private RelayCommand _addVoteCommand;
	public RelayCommand AddVoteCommand
	{
		get => _addVoteCommand ?? (_addVoteCommand = new RelayCommand(obj => SelectedPlayer.Votes++));
	}
	private RelayCommand _removeVoteCommand;
	public RelayCommand RemoveVoteCommand
	{
		get
		{
			return _removeVoteCommand ??
				(_removeVoteCommand = new RelayCommand(obj =>
				{
					SelectedPlayer.Votes--;
				}));
		}
	}
	private RelayCommand _voteCommand;
	public RelayCommand VoteCommand
	{
		get
		{
			return _voteCommand ??
				(_voteCommand = new RelayCommand(obj =>
				{
					var maxvotes = 0;
					var votedPlayers = new List<int>();
					foreach (var player in Players)
					{
						if (player.Votes > maxvotes)
						{
							maxvotes = player.Votes;
							votedPlayers = new List<int>
							{
								Players.IndexOf(player)
							};
						}
						else if (player.Votes == maxvotes)
						{
							votedPlayers.Add(Players.IndexOf(player));
						}
					}
					if (votedPlayers.Count > 1 && !IsSecondTour)
					{
						MessageBox.Show("Time for the second tour", "Vote", MessageBoxButton.OK, MessageBoxImage.Information);
						IsSecondTour = true;
						return;
					}
					string message;
					if (votedPlayers.Count == 1) message = $"{Players[votedPlayers[0]].Name} will be killed \n";
					else message = "nobody will be killed \n";
					if (MessageBox.Show( message +
							"Do you want to continue?",
							"Next stage",
							MessageBoxButton.YesNo,
							MessageBoxImage.Question) == MessageBoxResult.No) return;
					if(votedPlayers.Count == 1) Players[votedPlayers[0]].Kill();
					IsSecondTour = false;
					DayEnd();
				}));
		}
	}
	void DayEnd()
	{
		_discordClient.SendStatus(Players);
		_playersViewModel.ClearKilled();
		if (_windowModel.IsGameOver()) return;
		_playersViewModel.ClearStatus();
		_windowModel.SwitchCurrentViewModelTo<NightViewModel>();
	}
}
