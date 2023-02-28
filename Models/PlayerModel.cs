using Discord;
using Mafia_panel.Core;
using Mafia_panel.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Mafia_panel.Models
{
    /// <summary>
    /// Status of a player
    /// </summary>
    enum PlayerStatus
    {
        None,       
        Stunned0,
        Stunned1,
        Defended,   
        Killed
    }
    /// <summary>
    /// Player game role
    /// </summary>
    public enum PlayerRole
    {
        None,
        Civilian,
        Mafiozo,
        Godfather,
        Doctor,
        Prostitute,
        Chief,          
        Psychopath
    }
    class Player : ViewModelBase
    {
        public IUser User;
        private ulong _id;
        public ulong Id 
        {
            get => _id;
            set => SetProperty(ref _id, value); 
        }
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        private PlayerStatus _status = PlayerStatus.None;
        public PlayerStatus Status
        {
            get => _status;
            set
            {
                SetProperty(ref _status, value);
                if(Status != PlayerStatus.None)User.SendMessageAsync($"You {Status}");
            }
                
        }
        private PlayerRole _role;
        public PlayerRole Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }
        private int _votes;
        public int Votes
        {
            get => _votes;
            set => SetProperty(ref _votes, value);
        }
        public void TryKill(GameModeModel mods)
        {
            if (!(Status == PlayerStatus.Defended))
            {
                Kill();
            }
            else
            {
                Status = PlayerStatus.None;
                if (mods.IsDefenseStunning)Status = PlayerStatus.Stunned0;
            }
            
        }
        public virtual void Kill() => Status = PlayerStatus.Killed;
        public virtual void PlayerAction(Player target, GameModeModel mods, ref bool no_error)
        {
            if(Status == PlayerStatus.Stunned0 || Status == PlayerStatus.Stunned1)
            {
                no_error = true;
                return;
            }
        }
        public virtual void PlayerAlternativeAction(Player target, GameModeModel mods, ref bool no_error) => PlayerAction(target,mods,ref no_error);
}
    class Chief : Player
    {
        private int _kills=0;
        public int Kills
        {
            get => _kills;
            set => SetProperty(ref _kills, value);
        }
        private ObservableCollection<Player> _checkedPlayers;
        public ObservableCollection<Player> CheckedPlayers
        {
            get => _checkedPlayers;
            set => SetProperty(ref _checkedPlayers, value);
        }
        public Chief(Player player)
        {
            _checkedPlayers = new ObservableCollection<Player>();
            User = player.User;
            Id = player.Id;
            Name = player.Name;
            Role = PlayerRole.Chief;
            Status = player.Status;
        }
        /// <summary>
        /// Kills player
        /// </summary>
        /// <param name="target">Player to kill</param>
        /// <param name="mods">Game modificators</param>
        /// <param name="no_error">is function works right</param>
        public override void PlayerAction(Player target, GameModeModel mods, ref bool no_error)
        {
            base.PlayerAction(target, mods, ref no_error);
            if (!(mods.IsChiefLimitedKills && Kills == mods.ChiefLimitedKills) || !(mods.IsChiefCannotKillChecked && CheckedPlayers.Contains(target)))
            {
                target.TryKill(mods);
                Kills++;
                no_error = true;
                return;
            }
            else
            {
                MessageBox.Show("Cannot kill this target","Error", MessageBoxButton.OK, MessageBoxImage.Error);
                no_error = false;
                return;
            }
        }
        /// <summary>
        /// Checks player
        /// </summary>
        /// <param name="target">Player to check</param>
        /// <param name="mods">Game modificators</param>
        /// <param name="no_error">is function works right</param>
        public override void PlayerAlternativeAction(Player target, GameModeModel mods, ref bool no_error)
        {
            base.PlayerAction(target, mods, ref no_error);
            if (!CheckedPlayers.Contains(target))
            {
                User.SendMessageAsync($"<@{target.Id}> is {target.Role}");
                CheckedPlayers.Add(target);
                no_error = true;
                return;
            }
            else
            {
                MessageBox.Show("Cannot check this target", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                no_error = false;
                return;
            }
        }
    }
    class Resuscitator : Player
    {
        public Resuscitator(Player player)
        {
            User = player.User;
            Id = player.Id;
            Name = player.Name;
            Role = PlayerRole.Doctor;
            Status = player.Status;
        }
        private int _selfDefends = 0;
        public int SelfDefends
        {
            get => _selfDefends;
            set => SetProperty(ref _selfDefends, value);
        }
        /// <summary>
        /// Defends player
        /// </summary>
        /// <param name="target">Player to defend</param>
        /// <param name="mods">Game modificators</param>
        /// <param name="no_error">is function works right</param>
        public override void PlayerAction(Player target, GameModeModel mods, ref bool no_error)
        {
            base.PlayerAction(target, mods, ref no_error);
            if (!(this == target && SelfDefends > 0))
            {
                target.Status = PlayerStatus.Defended;
                if (this == target) SelfDefends++;
                no_error = true;
                return;
            }
            else
            {
                MessageBox.Show("Cannot defend this target", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                no_error = false;
                return;
            }
        }
    }
    class Anesthesiologist : Player
    {
        public Anesthesiologist(Player player)
        {
            User = player.User;
            Id = player.Id;
            Name = player.Name;
            Role = PlayerRole.Prostitute;
            Status = player.Status;
        }
        /// <summary>
        /// Stuns player
        /// </summary>
        /// <param name="target">Player to stun</param>
        /// <param name="mods">Game modificators</param>
        /// <param name="no_error">is function works right</param>
        public override void PlayerAction(Player target, GameModeModel mods, ref bool no_error)
        {
            base.PlayerAction(target, mods, ref no_error);
            if (target.Status == PlayerStatus.Defended || target.Status == PlayerStatus.Killed)
            {
                no_error = true;
                return;
            }
            target.Status = PlayerStatus.Stunned0;
            no_error = true;
        }
    }
    class Psychopath : Player
    {
        public Psychopath(Player player)
        {
            User = player.User;
            Id = player.Id;
            Name = player.Name;
            Role = PlayerRole.Psychopath;
            Status = player.Status;
        }
        /// <summary>
        /// Kills player
        /// </summary>
        /// <param name="target">Player to kill</param>
        /// <param name="mods">Game modificators</param>
        /// <param name="no_error">is function works right</param>
        public override void PlayerAction(Player target, GameModeModel mods, ref bool no_error)
        {
            base.PlayerAction(target, mods, ref no_error);
            target.TryKill(mods);
            no_error = true;
        }
    }
    class Curator : Player
    {
        private Action _curatorInherit;
        private ObservableCollection<Player> _checkedPlayers;
        public ObservableCollection<Player> CheckedPlayers
        {
            get => _checkedPlayers;
            set => SetProperty(ref _checkedPlayers, value);
        }
        public Curator(Player player, Action curatorInherit)
        {
            _curatorInherit = curatorInherit;
            _checkedPlayers = new ObservableCollection<Player>();
            User = player.User;
            Id = player.Id;
            Name = player.Name;
            Role = PlayerRole.Godfather;
            Status = player.Status;
        }
        /// <summary>
        /// Kills player
        /// </summary>
        /// <param name="target">Player to kill</param>
        /// <param name="mods">Game modificators</param>
        /// <param name="no_error">is function works right</param>
        public override void PlayerAction(Player target, GameModeModel mods, ref bool no_error)
        {
            base.PlayerAction(target, mods, ref no_error);
            target.TryKill(mods);
            no_error= true;
        }
        /// <summary>
        /// Checks player
        /// </summary>
        /// <param name="target">Player to check</param>
        /// <param name="mods">Game modificators</param>
        /// <param name="no_error">is function works right</param>
        public override void PlayerAlternativeAction(Player target, GameModeModel mods, ref bool no_error)
        {
            base.PlayerAction(target, mods, ref no_error);
            if (!(CheckedPlayers.Contains(target)) && mods.IsCuratorCanCheck)
            {
                User.SendMessageAsync($"<@{target.Id}> is {target.Role}");
                CheckedPlayers.Add(target);
                no_error = true;
                return;
            }
            else
            {
                MessageBox.Show("Cannot check this target", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                no_error = false;
                return;
            }
        }
        public override void Kill()
        {
            base.Kill();
            _curatorInherit.Invoke();
        }
    }
    /// <summary>
    /// Text about each role
    /// </summary>
    public static class Templates
    {
        public static readonly Dictionary<PlayerRole, string> RoleTemplates = new Dictionary<PlayerRole, string>
        {
            { PlayerRole.Chief, "" },
            { PlayerRole.Civilian, "" },
            { PlayerRole.Godfather,  "" },
            { PlayerRole.Mafiozo,  "" },
            { PlayerRole.Doctor, "" },
            { PlayerRole.Prostitute, "" },
        };
    }
}