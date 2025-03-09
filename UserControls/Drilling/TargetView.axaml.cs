using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

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
        FrameExpandImage.Path = $"avares://RFD/Assets/frame-expand-{theme}.svg";
        ExternalImage.Path = $"avares://RFD/Assets/external-{theme}.svg";
    }
    
    
    
}