using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace RFD.Controls;

public class ConditionBox : TemplatedControl
{
    public static readonly StyledProperty<string> HeaderProperty = AvaloniaProperty.Register<ConditionBox, string>(nameof(Header), "");
    public static readonly StyledProperty<bool> ConditionProperty = AvaloniaProperty.Register<ConditionBox, bool>(nameof(Condition), false);

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    public bool Condition
    {
        get => GetValue(ConditionProperty);
        set => SetValue(ConditionProperty, value);
    }
}