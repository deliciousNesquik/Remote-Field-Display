using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RFD.ViewModels;

namespace RFD.UserControls;

public partial class ManualConnectionDialog : UserControl
{
    public ManualConnectionDialog()
    {
        InitializeComponent();
        ChangeImageTheme(App.Instance.ActualThemeVariant.Key.ToString());
        App.Instance.ThemeChanged += ChangeImageTheme;
    }
    private void ChangeImageTheme(string? theme)
    {
        InternetConnectionImage.Path = $"../Assets/internet-connection-{theme}.svg";
    }
}