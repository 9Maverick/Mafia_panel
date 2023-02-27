using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Mafia_panel.Core;
using Mafia_panel.Models;

namespace Mafia_panel.ViewModels
{
	class MainViewModel : ViewModelBase
	{
		IDiscordSend _discordClient;
		ObservableCollection<Player> _players;
		ObservableCollection<Player> _playersBackup;
		ModeModel _mode;
		ViewModelBase _currentViewModel;
		public ViewModelBase CurrentViewModel
		{
			get => _currentViewModel;
			set => SetProperty(ref _currentViewModel, value);
		}

		Window _window;

		public MainViewModel(Window window)
		{
			_discordClient = new HollowDiscord();//DiscordClientModel();
			_players = new ObservableCollection<Player>();
			_mode = new ModeModel();
			CurrentViewModel = new InitialViewModel(_players, _mode, InitialSave, InitialConfigure);
			_window = window;	
		}

		void Inherit()
		{
			var selectedPlayers = new List<int>();
			var seed = new Random();
			var random = new Random(seed.Next());
			foreach (Player player in _players) if (player.Role == PlayerRole.Mafiozo) selectedPlayers.Add(_players.IndexOf(player));
			if (selectedPlayers.Count == 0) return;
			var curatorNum = selectedPlayers[random.Next(selectedPlayers.Count)];
			_players[curatorNum] = new Curator(_players[curatorNum], Inherit);
		}
		void InitialSave()
		{
			_playersBackup = new ObservableCollection<Player>(_players);
			_discordClient.SendInitialStatus(_players, _mode);
			for (int i = 0; i < _players.Count; i++)
			{
				switch (_players[i].Role)
				{
					case PlayerRole.Godfather:
						_players[i] = new Curator(_players[i], Inherit);
						break;
					case PlayerRole.Doctor:
						_players[i] = new Resuscitator(_players[i]);
						break;
					case PlayerRole.Prostitute:
						_players[i] = new Anesthesiologist(_players[i]);
						break;
					case PlayerRole.Chief:
						_players[i] = new Chief(_players[i]);
						break;
					case PlayerRole.Psychopath:
						_players[i] = new Psychopath(_players[i]);
						break;
				}
			}
			CurrentViewModel = new DayViewModel(DayEnd, _players);
		}
		void InitialConfigure() => _discordClient.ConfigurePlayers(_players);
		bool IsGameOver()
		{
			var badGuys = 0; var goodGuys = 0; var psycho = 0;
			foreach (var player in _players)
			{
				if (player.Role == PlayerRole.Mafiozo || player.Role == PlayerRole.Godfather) badGuys++;
				else if (player.Role == PlayerRole.Psychopath) psycho++;
				else goodGuys++;
			}
			if (badGuys > goodGuys + psycho)
			{
				Win(true);
				return true;
			}
			else if (badGuys + psycho == 0)
			{
				Win(false);
				return true;
			}
			return false;
		}
		void Win(bool isMafiaWins)
		{
			if (isMafiaWins) _discordClient.Send("<@&977568272029478962> Game over, bad guys wins", 915884902401073152);
			else _discordClient.Send("<@&977568272029478962> Game over, good guys wins", 915884902401073152);
			_players = new ObservableCollection<Player>(_playersBackup);
			_discordClient.SendStatus(_playersBackup, 915884902401073152);
			CurrentViewModel = new InitialViewModel(_players, _mode, InitialSave, InitialConfigure);
		}
		void DayEnd()
		{
			_discordClient.SendStatus(_players);
			var selectedPlayers = new List<int>();
			foreach (var player in _players) if (player.Status == PlayerStatus.Killed) selectedPlayers.Add(_players.IndexOf(player));
			foreach (var selectedPlayer in selectedPlayers) _players.Remove(_players[selectedPlayer]);
			if (IsGameOver()) return;
			foreach (var player in _players)
			{
				if (player.Status == PlayerStatus.Stunned0) player.Status = PlayerStatus.Stunned1;
				else player.Status = PlayerStatus.None;
				player.Votes = 0;
			}
			CurrentViewModel = new NightViewModel(NightEnd, _players, _discordClient, _mode);
		}
		void NightEnd()
		{
			_discordClient.SendStatus(_players);
			var selectedPlayers = new List<int>();
			foreach (var player in _players) if (player.Status == PlayerStatus.Killed) selectedPlayers.Add(_players.IndexOf(player));
			foreach (var selectedPlayer in selectedPlayers) _players.Remove(_players[selectedPlayer]);
			if (IsGameOver()) return;
			foreach (var player in _players)
			{
				if (player.Status == PlayerStatus.Stunned0) player.Status = PlayerStatus.Stunned1;
				else player.Status = PlayerStatus.None;
			}
			CurrentViewModel = new DayViewModel(DayEnd, _players);
		}

		private RelayCommand _minimizeCommand;
		public RelayCommand MinimizeCommand
		{
			get => _minimizeCommand ?? (_minimizeCommand = new RelayCommand(obj =>	_window.WindowState= WindowState.Minimized));
		}

		private RelayCommand _maximizeCommand;
		public RelayCommand MaximizeCommand
		{
			get => _maximizeCommand ?? (_maximizeCommand = new RelayCommand(obj => _window.WindowState ^= WindowState.Maximized));
		}

		private RelayCommand _closeCommand;
		public RelayCommand CloseCommand
		{
			get => _closeCommand ?? (_closeCommand = new RelayCommand(obj => _window.Close()));
		}
	}
}
