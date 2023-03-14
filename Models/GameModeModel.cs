using Mafia_panel.Core;

namespace Mafia_panel.Models;

public interface IGameModeModel
{
	int ChiefLimitedKills { get; set; }
	bool IsChiefCannotKillChecked { get; set; }
	bool IsChiefLimitedKills { get; set; }
	bool IsGodfatherCanCheck { get; set; }
	bool IsDefenseStunning { get; set; }
}

public class GameModeModel : ViewModelBase, IGameModeModel
{
	private bool _isDefenseStunning = false;
	public bool IsDefenseStunning
	{
		get => _isDefenseStunning;
		set => SetProperty(ref _isDefenseStunning, value);
	}
	private bool _isCuratorCanCheck = false;
	public bool IsGodfatherCanCheck
	{
		get => _isCuratorCanCheck;
		set => SetProperty(ref _isCuratorCanCheck, value);
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
