using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace RFD.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                bool isConnected => isConnected ? new SolidColorBrush(Color.FromRgb(52, 199, 89))  : new SolidColorBrush(Color.FromRgb(255, 59, 48)),
                _ => null
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}