using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RFD.UserControls;

public partial class InformationSection : UserControl
{
    public InformationSection()
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