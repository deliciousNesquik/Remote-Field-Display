using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace RFD.Converters;

public class NegativeHalfConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double size)
            return -size / 2;
        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}