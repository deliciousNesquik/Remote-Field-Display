using Avalonia;
using Avalonia.Controls.Primitives;

namespace RFD.Controls;

public class InfoBox : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<InfoBox, string>(nameof(Title), "");

    public static readonly StyledProperty<string> ContentProperty =
        AvaloniaProperty.Register<InfoBox, string>(nameof(Content), "");

    public static readonly StyledProperty<string> InscriptionProperty =
        AvaloniaProperty.Register<InfoBox, string>(nameof(Inscription), "");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public string Inscription
    {
        get => GetValue(InscriptionProperty);
        set => SetValue(InscriptionProperty, value);
    }
}