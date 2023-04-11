using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mafia_panel.Core;

/// <summary>
/// Base for view models with implementation of <see cref="INotifyPropertyChanged"/>
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = "")
    {
        if (Equals(property,value)) return;
        property = value;
        OnPropertyChanged(propertyName);
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
