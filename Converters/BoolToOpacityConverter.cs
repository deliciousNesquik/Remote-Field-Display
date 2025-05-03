using System.Globalization;
using Avalonia.Data.Converters;

namespace RFD.Converters;

public class BoolToOpacityConverter : IValueConverter
{
    public static BoolToOpacityConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool and true ? 1.0 : 0.7;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}