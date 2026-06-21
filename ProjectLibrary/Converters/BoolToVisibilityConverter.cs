using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProjectLibrary.Converters;

/// <summary>
/// Standard bool -> Visibility converter with an inverse mode via ConverterParameter.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public static readonly BoolToVisibilityConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var flag = value is bool b && b;
        var invert = parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase);
        if (invert) flag = !flag;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Visibility v && v == Visibility.Visible;
}
