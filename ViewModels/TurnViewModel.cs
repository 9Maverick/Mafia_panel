using Mafia_panel.Models;
using Mafia_panel.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;


namespace Mafia_panel.ViewModels
{
    class TurnViewModel : ViewModelBase
    {
        Action _nextTurn;
        IDiscordSend _discordClient;
        ObservableCollection<Player> _players;
        public ObservableCollection<Player> Players
        {
            get => _players;
            set => SetProperty(ref _players, value);
        }
        ModeModel _mode;
        Player _actorPlayer;
        public Player ActorPlayer
        {
            get => _actorPlayer;
            set => SetProperty(ref _actorPlayer, value);
        }
        PlayerRole _actorPlayerRole;
        public PlayerRole ActorPlayerRole
        {
            get => _actorPlayerRole;
            set => SetProperty(ref _actorPlayerRole, value);
        }
        private string _actorPlayerRoleName;
        public string ActorPlayerRoleName
        {
            get => _actorPlayerRoleName;
            set => SetProperty(ref _actorPlayerRoleName, value);
        }
        Player _targetPlayer;
        public Player TargetPlayer
        {
            get => _targetPlayer;
            set => SetProperty(ref _targetPlayer, value);
        }
        private string _actionName = "Kill";
        public string ActionName
        {
            get => _actionName;
            set => SetProperty(ref _actionName, value);
        }
        private string _alternativeActionName = "Check";
        public string AlternativeActionName
        {
            get => _alternativeActionName;
            set => SetProperty(ref _alternativeActionName, value);
        }
        private bool _isAlternativeActionVisible=false;
        public bool IsAlternativeActionVisible
        {
            get => _isAlternativeActionVisible;
            set => SetProperty(ref _isAlternativeActionVisible, value);
        }
        public TurnViewModel(ObservableCollection<Player> players, ModeModel mode, PlayerRole actorPlayerRole, Action nextTurn, IDiscordSend discordClient)
        {
            _nextTurn = nextTurn;
            _discordClient = discordClient;
            Players = players;
            _mode = mode;
            ActorPlayerRole = actorPlayerRole;
            ActorPlayerRoleName = ActorPlayerRole.ToString();
            foreach (var player in Players) if (player.Role == ActorPlayerRole) ActorPlayer = player;
            switch(ActorPlayerRole)
            {
                case PlayerRole.Chief:
                    IsAlternativeActionVisible = true;
                    break;
                case PlayerRole.Resuscitator:
                    ActionName = "Defend";
                    break;
                case PlayerRole.Anesthesiologist:
                    ActionName = "Stun";
                    break;
                case PlayerRole.Curator:
                    IsAlternativeActionVisible = _mode.IsCuratorCanCheck;
                    break;
            }
            TargetPlayer = Players[0];
            string message = "";
            for(int i = 0; i < Players.Count; i++ ) message += $"{i+1}. {Players[i].Name}" + "\n";
            _discordClient.SendToUserById(ActorPlayer.Id, "Your Turn, choose target by \"!target <number of target>\" Example:\n!target 3\n" + "Targets:\n" + message);
        }
        private RelayCommand _actionCommand;
        public RelayCommand ActionCommand
        {
            get
            {
                return _actionCommand ??
                    (_actionCommand = new RelayCommand(obj =>
                    {
                        if (MessageBox.Show($"{TargetPlayer.Name} will be {ActionName}ed \n" +
                                "Do you want to continue?",
                                "Next Turn",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.No) return;
                        bool no_trouble = false;
                        ActorPlayer.PlayerAction(TargetPlayer, _mode, ref no_trouble);
                        if (!no_trouble) return;
                        _nextTurn.Invoke();
                    }));
            }
        }
        private RelayCommand _altenativeActionCommand;
        public RelayCommand AltenativeActionCommand
        {
            get
            {
                return _altenativeActionCommand ??
                    (_altenativeActionCommand = new RelayCommand(obj =>
                    {
                        if (MessageBox.Show($"{TargetPlayer.Name} will be {AlternativeActionName}ed \n" +
                                "Do you want to continue?",
                                "Next Turn",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.No) return;
                        bool no_trouble = false;
                        ActorPlayer.PlayerAlternativeAction(TargetPlayer, _mode, ref no_trouble);
                        if (!no_trouble) return;
                        _nextTurn.Invoke();
                    }));
            }
        }
    }
}
