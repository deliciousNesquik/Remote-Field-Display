using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace RFD.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isConnected)
            {
                // Определяем ключи для ресурсов
                var resourceKey = isConnected ? "GreenColor" : "RedColor";

                // Получаем текущие ресурсы приложения
                if (Application.Current?.Resources.TryGetValue(resourceKey, out var resource) == true)
                {
                    if (resource is Color color)
                    {
                        return new SolidColorBrush(color);
                    }
                    return resource; // Если ресурс уже является кистью
                }
            }

            // Цвет по умолчанию, если ресурс не найден
            return Brushes.Gray;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}