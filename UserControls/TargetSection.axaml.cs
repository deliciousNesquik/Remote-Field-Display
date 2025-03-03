using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RFD.UserControls;

public partial class TargetSection : UserControl
{
    public TargetSection()
    {
        InitializeComponent();
        ChangeImageTheme(App.Instance.ActualThemeVariant.Key.ToString());
        App.Instance.ThemeChanged += ChangeImageTheme;
    }
    private void ChangeImageTheme(string? theme)
    {
        FrameExpandImage.Path = $"../Assets/frame-expand-{theme}.svg";
        ExternalImage.Path = $"../Assets/external-{theme}.svg";
    }
    
}