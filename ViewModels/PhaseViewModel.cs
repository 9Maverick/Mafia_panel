using Mafia_panel.Core;

namespace Mafia_panel.ViewModels;

public abstract class PhaseViewModel : NotifyPropertyChanged
{
	public abstract void OnStart();
	public abstract void OnEnd();
}
