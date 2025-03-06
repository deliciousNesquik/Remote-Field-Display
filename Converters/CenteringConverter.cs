using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RFD.Converters;

public class CenteringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Позиционирование относительно центра Canvas
        double radius = (double)value;
        return 200 - radius; // Здесь 200 - это центр канваса, вы можете изменить это значение в зависимости от размеров вашего Canvas
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}