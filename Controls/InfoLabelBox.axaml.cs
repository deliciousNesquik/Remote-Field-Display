using Avalonia;
using Avalonia.Controls.Primitives;
using RFD.Controls;

namespace RFD.Controls;

public class InfoLabelBox : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<InfoLabelBox, string>(nameof(Title), "Текст");
    public static readonly StyledProperty<string> ContentProperty = AvaloniaProperty.Register<InfoLabelBox, string>(nameof(Content), "—");
    public static readonly StyledProperty<string> InscriptionProperty = AvaloniaProperty.Register<InfoLabelBox, string>(nameof(Inscription), "м");

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