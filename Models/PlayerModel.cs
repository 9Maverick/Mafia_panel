using Discord;
using Discord.WebSocket;
using Mafia_panel.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Mafia_panel.Models;

/// <summary>
/// Status of a player
/// </summary>
public enum PlayerStatus
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
	Anesthesiologist,
	Chief,          
	Psychopath
}
public class Player : ViewModelBase
{
	private SocketGuildUser _user;
	public SocketGuildUser User
	{
		get => _user;
		set
		{
			SetProperty(ref _user, value);
			if (User == null) return;
			Name = User.Nickname ?? User.Username;
			Id = User.Id;
		}
	}
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

	public Player(){}
	public Player(Player player)
	{
		User = player.User ?? null;
		Id = player.Id;
		Name = player.Name;
		Role = player.Role;
		Status = player.Status;
	}

	public void TryKill(IGameModeModel mods)
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
	public virtual bool PlayerAction(Player target, IGameModeModel mods)
	{
		if(Status == PlayerStatus.Stunned0 || Status == PlayerStatus.Stunned1)
		{
			return false;
		}
		return true;
	}
	public virtual bool PlayerAlternativeAction(Player target, IGameModeModel mods) => PlayerAction(target,mods);
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
	public ObservableCollection<Player> CheckedPlayers => _checkedPlayers;
	public Chief(Player player) : base(player)
	{
		_checkedPlayers = new ObservableCollection<Player>();
		Role = PlayerRole.Chief;
	}
	/// <summary>
	/// Kills player
	/// </summary>
	/// <param name="target">Player to kill</param>
	/// <param name="mods">Game modifiers</param>
	public override bool PlayerAction(Player target, IGameModeModel mods)
	{
		bool canPerform = base.PlayerAction(target, mods);
		if (!(mods.IsChiefLimitedKills && Kills == mods.ChiefLimitedKills) || !(mods.IsChiefCannotKillChecked && CheckedPlayers.Contains(target)) && canPerform)
		{
			target.TryKill(mods);
			Kills++;
			return canPerform;
		}

		MessageBox.Show("Cannot kill this target", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		return false;
	} 
	/// <summary>
	/// Checks player
	/// </summary>
	/// <param name="target">Player to check</param>
	/// <param name="mods">Game modifiers</param>
	public override bool PlayerAlternativeAction(Player target, IGameModeModel mods)
	{
		bool canPerform =  base.PlayerAlternativeAction(target, mods);
		if (!CheckedPlayers.Contains(target) && canPerform)
		{
			User.SendMessageAsync($"<@{target.Id}> is {target.Role}");
			CheckedPlayers.Add(target);
			return canPerform;
		}

		MessageBox.Show("Cannot check this target", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		return false;
	}
}
class Doctor : Player
{
	public Doctor(Player player) : base(player)
	{
		Role = PlayerRole.Doctor;
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
	/// <param name="mods">Game modifiers</param>
	public override bool PlayerAction(Player target, IGameModeModel mods)
	{
		bool canPerform = base.PlayerAction(target, mods);
		if (!(this == target && SelfDefends > 0) && canPerform)
		{
			target.Status = PlayerStatus.Defended;
			if (this == target) SelfDefends++;
			return canPerform;
		}

		MessageBox.Show("Cannot defend this target", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		return false;
	}
}
class Anesthesiologist : Player
{
	public Anesthesiologist(Player player) : base(player) 
	{
		Role = PlayerRole.Anesthesiologist;
	}
	/// <summary>
	/// Stuns player
	/// </summary>
	/// <param name="target">Player to stun</param>
	/// <param name="mods">Game modifiers</param>
	public override bool PlayerAction(Player target, IGameModeModel mods)
	{
		bool canPerform = base.PlayerAction(target, mods);
		if (!(target.Status == PlayerStatus.Defended || target.Status == PlayerStatus.Killed) && canPerform)
		{
			target.Status = PlayerStatus.Stunned0;
		}
		return canPerform;
	}
}
class Psychopath : Player
{
	public Psychopath(Player player): base(player) 
	{
		Role = PlayerRole.Psychopath; 
	}
	/// <summary>
	/// Kills player
	/// </summary>
	/// <param name="target">Player to kill</param>
	/// <param name="mods">Game modifiers</param>
	public override bool PlayerAction(Player target, IGameModeModel mods)
	{
		bool canPerform = base.PlayerAction(target, mods);
		if(canPerform)
			target.TryKill(mods);
		return canPerform;
	}
}
class Godfather : Player
{
	private ObservableCollection<Player> _checkedPlayers;
	public ObservableCollection<Player> CheckedPlayers
	{
		get => _checkedPlayers;
		set => SetProperty(ref _checkedPlayers, value);
	}
	public Godfather(Player player) : base(player) 
	{
		_checkedPlayers = new ObservableCollection<Player>();
		Role = PlayerRole.Godfather;
	}
	/// <summary>
	/// Kills player
	/// </summary>
	/// <param name="target">Player to kill</param>
	/// <param name="mods">Game modifiers</param>
	public override bool PlayerAction(Player target, IGameModeModel mods)
	{
		bool canPerform = base.PlayerAction(target, mods);
		if (canPerform)
			target.TryKill(mods);
		return canPerform;
	}
	/// <summary>
	/// Checks player
	/// </summary>
	/// <param name="target">Player to check</param>
	/// <param name="mods">Game modifiers</param>
	public override bool PlayerAlternativeAction(Player target, IGameModeModel mods)
	{
		bool canPerform = base.PlayerAlternativeAction(target, mods);
		if (!CheckedPlayers.Contains(target) && mods.IsGodfatherCanCheck && canPerform)
		{
			User.SendMessageAsync($"<@{target.Id}> is {target.Role}");
			CheckedPlayers.Add(target);
			return canPerform;
		}

		MessageBox.Show("Cannot check this target", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		return false;
	}
	public override void Kill()
	{
		base.Kill();
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
		{ PlayerRole.Anesthesiologist, "" },
	};
}