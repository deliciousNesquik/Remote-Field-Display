using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace RFD.Converters
{
    public class BoolToStrokeDashConverter : MarkupExtension, IValueConverter
    {
        private static BoolToStrokeDashConverter? _instance;
        
        public static BoolToStrokeDashConverter Instance => _instance ??= new BoolToStrokeDashConverter();
        
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && !b) 
                ? new Avalonia.Collections.AvaloniaList<double> { 2, 2 } 
                : new Avalonia.Collections.AvaloniaList<double>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}