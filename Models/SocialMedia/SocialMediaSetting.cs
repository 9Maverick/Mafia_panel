using Mafia_panel.Core;
using System;
using System.Collections.ObjectModel;

namespace Mafia_panel.Models.SocialMedia;

public enum ControlType
{
    None,
    TextBox,
    ComboBox
}

public class SocialMediaSetting : NotifyPropertyChanged
{
    public readonly Type Type;
    Action? _valueChanged;
    public readonly ControlType _control;
    public ControlType Control
    {
        get => _control;
    }
    private object? _value;
    public object? Value
    {
        get => _value;
        set
        {
            SetValue(ref _value, value);
            if (_valueChanged == null) return;
            _valueChanged.Invoke();
        }
    }
    private ObservableCollection<object>? _source;
    public ObservableCollection<object>? Source
    {
        get => _source;
        set => SetValue(ref _source, value);
    }

    public SocialMediaSetting(Type settingType, ControlType control, Action? valueChanged = null)
    {
        Type = settingType;
        _control = control;
        _valueChanged = valueChanged;
        _source = new ObservableCollection<object>();
    }
}
