using Mafia_panel.Models;
using Mafia_panel.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;

namespace Mafia_panel.ViewModels
{
	internal class DayViewModel : ViewModelBase
	{
		Action _dayEnd;
		PlayersViewModel _playersViewModel;
		ObservableCollection<Player> _players;
		public ObservableCollection<Player> Players => _playersViewModel.Players;
		Player _selectedPlayer;
		public Player SelectedPlayer
		{
			get => _selectedPlayer;
			set => SetProperty(ref _selectedPlayer, value);
		}
		private bool _isSecondTour = false;
		public bool IsSecondTour
		{
			get => _isSecondTour;
			set => SetProperty(ref _isSecondTour, value);
		}

		public DayViewModel(Action dayEnd, PlayersViewModel playersViewModel)
		{
			_dayEnd = dayEnd;
			_playersViewModel = playersViewModel;
			SelectedPlayer = Players.FirstOrDefault();
		}

		private RelayCommand _addVoteCommand;
		public RelayCommand AddVoteCommand
		{
			get 
			{
				return _addVoteCommand ??
					(_addVoteCommand = new RelayCommand(obj =>
					{
						SelectedPlayer.Votes++;
					}));
			}
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
								votedPlayers = new List<int>();
								votedPlayers.Add(Players.IndexOf(player));
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
						_dayEnd.Invoke();
					}));
			}
		}
	}
}
