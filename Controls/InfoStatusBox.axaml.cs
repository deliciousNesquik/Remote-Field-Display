using Avalonia;
using Avalonia.Controls.Primitives;

namespace RFD.Controls;

public class InfoStatusBox : TemplatedControl
{
    public static readonly StyledProperty<string> HeaderProperty = AvaloniaProperty.Register<InfoLabelBox, string>(nameof(Header), "Заголовок");

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}