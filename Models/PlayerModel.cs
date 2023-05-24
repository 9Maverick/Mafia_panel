using Mafia_panel.Core;
using Mafia_panel.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mafia_panel.Models;

/// <summary>
/// Statuses of a player
/// </summary>
public enum PlayerStatus
{
	None,       
	StunnedNight,
	StunnedDay,
	Defended,   
	Killed
}
/// <summary>
/// Player game roles
/// </summary>
public enum PlayerRole
{
	None,
	Civilian,
	Mafioso,
	Godfather,
	Doctor,
	Lady,
	Chief,          
	Psychopath
}
public class Player : ViewModelBase
{
	private ISocialMediaUser? _user;
	public ISocialMediaUser? User
	{
		get => _user;
		set => SetValue(ref _user, value);
	}
	private string _name;
	public string Name
	{
		get => _name;
		set => SetValue(ref _name, value);
	}
	private PlayerStatus _status = PlayerStatus.None;
	public PlayerStatus Status
	{
		get => _status;
		set
		{
			SetValue(ref _status, value);
			if (Status != PlayerStatus.None && User != null)
			{
				User.SendMessage($"You {Status}");
			}
		}
			
	}
	private PlayerRole _role;
	public PlayerRole Role
	{
		get => _role;
		set => SetValue(ref _role, value);
	}
	private uint _votes;
	/// <summary>
	/// How many players voted to kill this player
	/// </summary>
	public uint Votes
	{
		get => _votes;
		set => SetValue(ref _votes, value);
	}
	bool _canVote = false;
	public bool CanVote
	{
		get => _canVote;
		set => SetValue(ref _canVote, value);
	}
	bool _canAct = false;
	public bool CanAct
	{
		get => _canAct;
		set => SetValue(ref _canAct, value);
	}

	public Player(){}
	public Player(Player player)
	{
		User = player.User;
		Name = player.Name;
		Role = player.Role;
		Status = player.Status;
	}
	public Player(ISocialMediaUser user) 
	{ 
		User = user;
		Name = user.Name;
	}
	/// <summary>
	/// Kills player if his status is not <see cref="PlayerStatus.Defended"/>
	/// </summary>
	/// <param name="mods">game rules</param>
	public void TryKill(IGameRulesModel mods)
	{
		if (!(Status == PlayerStatus.Defended))
		{
			Kill();
			return;
		}
		Status = PlayerStatus.None;
		if (mods.IsDefenseStunning) Status = PlayerStatus.StunnedNight;

	}
	/// <summary>
	/// Changes status of player to <see cref="PlayerStatus.Killed"/>
	/// </summary>
	public virtual void Kill() => Status = PlayerStatus.Killed;
	public virtual bool PlayerAction(Player target, IGameRulesModel mods)
	{
		return (Status == PlayerStatus.StunnedNight || Status == PlayerStatus.StunnedDay) ? false : true;
	}
	public virtual bool PlayerAlternativeAction(Player target, IGameRulesModel mods)
	{
		return (Status == PlayerStatus.StunnedNight || Status == PlayerStatus.StunnedDay) ? false : true;
	}
}
class Chief : Player
{
	private int _kills=0;
	/// <summary>
	/// How many players were killed
	/// </summary>
	public int Kills
	{
		get => _kills;
		set => SetValue(ref _kills, value);
	}
	private ObservableCollection<Player> _checkedPlayers;
	/// <summary>
	/// List of players that had been checked
	/// </summary>
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
	public override bool PlayerAction(Player target, IGameRulesModel mods)
	{
		bool canPerform = base.PlayerAction(target, mods);
		if (!(mods.IsChiefLimitedKills && Kills == mods.ChiefLimitedKills) && !(mods.IsChiefCannotKillChecked && CheckedPlayers.Contains(target)) && canPerform)
		{
			target.TryKill(mods);
			Kills++;
		}
		return canPerform;
	} 
	/// <summary>
	/// Checks player role
	/// </summary>
	/// <param name="target">Player to check</param>
	/// <param name="mods">Game modifiers</param>
	public override bool PlayerAlternativeAction(Player target, IGameRulesModel mods)
	{
		bool canPerform =  base.PlayerAlternativeAction(target, mods);
		if (!CheckedPlayers.Contains(target) && canPerform)
		{
			if(User != null) User.SendMessage($"{target.Name} is {target.Role}");
			CheckedPlayers.Add(target);
			return true;
		}
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
	/// <summary>
	/// How many times <see cref="Doctor"/> defended himself
	/// </summary>
	public int SelfDefends
	{
		get => _selfDefends;
		set => SetValue(ref _selfDefends, value);
	}
	/// <summary>
	/// Defends player
	/// </summary>
	/// <param name="target">Player to defend</param>
	/// <param name="mods">Game modifiers</param>
	public override bool PlayerAction(Player target, IGameRulesModel mods)
	{
		bool canPerform = base.PlayerAction(target, mods);
		if (!(this == target && SelfDefends > 0) && canPerform)
		{
			target.Status = PlayerStatus.Defended;
			if (this == target) SelfDefends++;
		}
		return canPerform;
	}
}
class Lady : Player
{
	public Lady(Player player) : base(player) 
	{
		Role = PlayerRole.Lady;
	}
	/// <summary>
	/// Stuns player
	/// </summary>
	/// <param name="target">Player to stun</param>
	/// <param name="mods">Game modifiers</param>
	public override bool PlayerAction(Player target, IGameRulesModel mods)
	{
		bool canPerform = base.PlayerAction(target, mods);
		if (!(target.Status == PlayerStatus.Defended || target.Status == PlayerStatus.Killed) && canPerform)
		{
			target.Status = PlayerStatus.StunnedNight;
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
	public override bool PlayerAction(Player target, IGameRulesModel mods)
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
	/// <summary>
	/// List of players that had been checked
	/// </summary>
	public ObservableCollection<Player> CheckedPlayers
	{
		get => _checkedPlayers;
		set => SetValue(ref _checkedPlayers, value);
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
	public override bool PlayerAction(Player target, IGameRulesModel mods)
	{
		bool canPerform = base.PlayerAction(target, mods);
		if (canPerform)
			target.TryKill(mods);
		return canPerform;
	}
	/// <summary>
	/// Checks player role
	/// </summary>
	/// <param name="target">Player to check</param>
	/// <param name="mods">Game modifiers</param>
	public override bool PlayerAlternativeAction(Player target, IGameRulesModel mods)
	{
		bool canPerform = base.PlayerAlternativeAction(target, mods);
		if (!CheckedPlayers.Contains(target) && mods.IsGodfatherCanCheck && canPerform)
		{
			if (User != null) User.SendMessage($"{target.Name} is {target.Role}");
			CheckedPlayers.Add(target);
			return true;
		}
		return false;
	}
}

/// <summary>
/// Text about each role
/// </summary>
public static class Templates
{
	public static readonly Dictionary<PlayerRole, string> RoleTemplates = new Dictionary<PlayerRole, string>
	{
		{ PlayerRole.Chief, "You chief" },
		{ PlayerRole.Civilian, "You civilian" },
		{ PlayerRole.Godfather,  "You Godfather" },
		{ PlayerRole.Mafioso,  "You Mafioso" },
		{ PlayerRole.Doctor, "You Doctor" },
		{ PlayerRole.Lady, "You Lady" },
	};
}