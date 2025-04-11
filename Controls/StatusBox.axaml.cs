using Avalonia;
using Avalonia.Controls.Primitives;

namespace RFD.Controls;

public class StatusBox : TemplatedControl
{
    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<StatusBox, string>(nameof(Header), "");

    public static readonly StyledProperty<bool> StatusProperty =
        AvaloniaProperty.Register<StatusBox, bool>(nameof(Status));

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public bool Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
}