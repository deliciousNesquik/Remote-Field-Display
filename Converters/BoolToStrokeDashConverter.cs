using System.Globalization;
using Avalonia.Collections;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace RFD.Converters;

public class BoolToStrokeDashConverter : MarkupExtension, IValueConverter
{
    private static BoolToStrokeDashConverter? _instance;

    public static BoolToStrokeDashConverter Instance => _instance ??= new BoolToStrokeDashConverter();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool and false
            ? new AvaloniaList<double> { 2, 2 }
            : new AvaloniaList<double>();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}