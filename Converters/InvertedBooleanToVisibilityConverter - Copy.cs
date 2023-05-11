using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mafia_panel.Converters;

class InvertedBooleanToVisibilityConverter : IValueConverter
{
	/// <summary>
	/// Converting inverted boolean to Visibility for controls
	/// </summary>
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (!(value is bool)) return null;
		return (bool)value ? Visibility.Collapsed : Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
