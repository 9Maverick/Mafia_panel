using Mafia_panel.Models.SocialMedia;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mafia_panel.Converters;

class TextBoxControlTypeToVisibilityConverter : IValueConverter
{
	/// <summary>
	/// Converting inverted boolean to Visibility for controls
	/// </summary>
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (!(value is ControlType)) return null;
		return (ControlType)value == ControlType.TextBox? Visibility.Visible : Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
