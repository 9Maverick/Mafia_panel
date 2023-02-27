using Mafia_panel.Models;
using Mafia_panel.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;


namespace Mafia_panel.ViewModels
{
    class NightViewModel : ViewModelBase
    {
        Action _nightEnd;
        IDiscordSend _discordClient;
        ObservableCollection<Player> _players;
        ModeModel _mode;
        TurnViewModel _turn;
        public TurnViewModel Turn
        {
            get => _turn;
            set => SetProperty(ref _turn, value);
        }

        public NightViewModel(Action nightEnd, ObservableCollection<Player> players, IDiscordSend discordClient, ModeModel mode)
        {
            _nightEnd = nightEnd;
            _players = players;
            _discordClient = discordClient;
            _mode = mode;
            ChangeTurn(PlayerRole.Godfather);
        }
        void ChangeTurn(PlayerRole role)
        {
            if ((int)role == 8)
            {
                _nightEnd.Invoke();
                return;
            }
            var playerExist = false;
            foreach (var player in _players) if (player.Role == role && player.Status != PlayerStatus.Stunned1) playerExist = true;
            if(playerExist)
            {
                _discordClient.Send($"{role}s Turn");
                Turn = new TurnViewModel(_players, _mode, role, () => ChangeTurn(role + 1), _discordClient);
                return;
            }
            ChangeTurn(role + 1);
        }
    }
}
