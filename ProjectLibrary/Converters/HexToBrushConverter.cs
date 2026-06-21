using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProjectLibrary.Converters;

/// <summary>
/// Converts a hex color string (e.g. "#E0115F") into a frozen SolidColorBrush.
/// Used to bind tech-badge brand colors and placeholder thumbnail backgrounds.
/// Returns transparent on parse failure so the UI fails soft.
/// </summary>
public class HexToBrushConverter : IValueConverter
{
    public static readonly HexToBrushConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hex && !string.IsNullOrWhiteSpace(hex))
        {
            try
            {
                var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
                brush.Freeze();
                return brush;
            }
            catch { /* fall through */ }
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
