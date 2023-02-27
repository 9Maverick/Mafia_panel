using Mafia_panel.Models;
using Mafia_panel.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Mafia_panel.ViewModels
{
	internal class InitialViewModel : ViewModelBase
	{
		Action _initialSave;
		Action _initialConfigure;
		ObservableCollection<Player> _players;
		private Player _selectedPlayer;
		public Player SelectedPlayer
		{
			get => _selectedPlayer;
			set => SetProperty(ref _selectedPlayer, value);
		}
		public ObservableCollection<Player> Players
		{
			get => _players;
			set => SetProperty(ref _players, value);
		}

		ModeModel _mode;
		public ModeModel Mode
		{
			get => _mode;
			set => SetProperty(ref _mode, value);
		}

		private bool _isRolesGiven = false;
		public bool IsRolesGiven
		{
			get => _isRolesGiven;
			set => SetProperty(ref _isRolesGiven, value);
		}
		public InitialViewModel(ObservableCollection<Player> players, ModeModel mod, Action initialSave, Action initialConfigure)
		{
			foreach (var player in players) player.Status = PlayerStatus.None;
			Players = players;
			Mode = mod;
			_initialSave = initialSave;
			_initialConfigure = initialConfigure;
		}

		private RelayCommand _addPlayerCommand;
		public RelayCommand AddPlayerCommand
		{
			get
			{
				return _addPlayerCommand ??
					(_addPlayerCommand = new RelayCommand(obj =>
					{
						Player player = new Player();
						Players.Add(player);
					}));
			}
		}
		private RelayCommand _removePlayerCommand;
		public RelayCommand RemovePlayerCommand
		{
			get => _removePlayerCommand ?? (_removePlayerCommand = new RelayCommand(obj => Players.Remove(SelectedPlayer)));
		}

		
		private RelayCommand _addRolesCommand;
		public RelayCommand AddRolesCommand
		{
			get
			{
				return _addRolesCommand ??
					(_addRolesCommand = new RelayCommand(obj =>
					{
						if (Players.Count < 3) return;
						foreach (Player player in Players) player.Role = PlayerRole.Civilian;

						var selectedPlayers = new List<int>();
						int vivisectorCount = Players.Count / 3;
						for (int i = 0; i < vivisectorCount; i++)
						{
							SetPlayerRole(PlayerRole.Mafiozo);
						}
						foreach (Player player in Players) if (player.Role == PlayerRole.Mafiozo) selectedPlayers.Add(Players.IndexOf(player));
						var curatorNum = selectedPlayers[GetRandomNumber(selectedPlayers.Count)];
						Players[curatorNum].Role = PlayerRole.Godfather;

						
						if (!(Players.Count < 6))   SetPlayerRole(PlayerRole.Doctor);

						if (!(Players.Count < 9))   SetPlayerRole(PlayerRole.Chief);

						if (!(Players.Count < 12))  SetPlayerRole(PlayerRole.Prostitute);

						if (!(Players.Count < 15))  SetPlayerRole(PlayerRole.Psychopath);

						IsRolesGiven = true;
						_initialConfigure.Invoke(); 
					}));
			}
		}
		int GetRandomNumber(int max)
		{
			var random = new Random();
			return random.Next(max);
		}
		void SetPlayerRole(PlayerRole role)
		{
			var selectedPlayers = new List<int>();
			foreach (Player player in Players) if (player.Role == PlayerRole.Civilian) selectedPlayers.Add(Players.IndexOf(player));
			var roleNum = selectedPlayers[GetRandomNumber(selectedPlayers.Count)];
			Players[roleNum].Role = role;
		}
		private RelayCommand _saveCommand;
		public RelayCommand SaveCommand
		{
			get => _saveCommand ?? (_saveCommand = new RelayCommand(obj =>_initialSave.Invoke()));
		}
	}
}