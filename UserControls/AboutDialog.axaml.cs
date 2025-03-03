using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RFD.UserControls;

public partial class AboutDialog : UserControl
{
    public AboutDialog()
    {
        InitializeComponent();
        ChangeImageTheme(App.Instance.ActualThemeVariant.Key.ToString());
        App.Instance.ThemeChanged += ChangeImageTheme;
    }
    private void ChangeImageTheme(string? theme)
    {
        HelpImage.Path = $"../Assets/help-{theme}.svg";
    }
}