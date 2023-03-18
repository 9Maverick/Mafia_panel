using Mafia_panel.Core;

namespace Mafia_panel.Models;

public interface IGameRulesModel
{
	/// <summary>
	/// Defines whenever <see cref="Chief"/> can kill players that he had checked
	/// </summary>
	bool IsChiefCannotKillChecked { get; set; }
	/// <summary>
	/// Defines whenever <see cref="Godfather"/> can check roles of other players like <see cref="Chief"/>
	/// </summary>
	bool IsGodfatherCanCheck { get; set; }
	/// <summary>
	/// Defines whenever after <see cref="Doctor"/> defend player becomes stunned
	/// </summary>
	bool IsDefenseStunning { get; set; }
	/// <summary>
	/// Defines whenever <see cref="Chief"/> can kill only limited amount of players
	/// </summary>
	bool IsChiefLimitedKills { get; set; }
	/// <summary>
	/// Number of players <see cref="Chief"/> can kill
	/// relevant only when <see cref="IsChiefLimitedKills"/> set to true
	/// </summary>
	int ChiefLimitedKills { get; set; }
}
/// <summary>
/// Container for game rules
/// </summary>
public class GameRulesModel : ViewModelBase, IGameRulesModel
{
	private bool _isDefenseStunning = false;
	public bool IsDefenseStunning
	{
		get => _isDefenseStunning;
		set => SetProperty(ref _isDefenseStunning, value);
	}
	private bool _isGodfatherCanCheck = false;
	public bool IsGodfatherCanCheck
	{
		get => _isGodfatherCanCheck;
		set => SetProperty(ref _isGodfatherCanCheck, value);
	}
	private bool _isChiefLimitedKills = false;
	public bool IsChiefLimitedKills
	{
		get => _isChiefLimitedKills;
		set => SetProperty(ref _isChiefLimitedKills, value);
	}
	private int _chiefLimitedKills = 0;
	public int ChiefLimitedKills
	{
		get => _chiefLimitedKills;
		set => SetProperty(ref _chiefLimitedKills, value);
	}
	private bool _isChiefCannotKillChecked = false;
	public bool IsChiefCannotKillChecked
	{
		get => _isChiefCannotKillChecked;
		set => SetProperty(ref _isChiefCannotKillChecked, value);
	}
}
