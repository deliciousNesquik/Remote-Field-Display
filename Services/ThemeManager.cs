using Avalonia;
using Avalonia.Markup.Xaml.Styling;

namespace RFD.Services;

public enum AppTheme
{
    Light,
    Dark
}

public static class ThemeManager
{
    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public static void ApplyTheme(AppTheme theme)
    {
        var app = Application.Current;
        if (app is null)
            return;

        var existingTheme = app.Resources.MergedDictionaries
            .OfType<ResourceInclude>()
            .FirstOrDefault(x => x.Source?.OriginalString.Contains("Theme") == true);

        if (existingTheme != null)
            app.Resources.MergedDictionaries.Remove(existingTheme);

        var themePath = theme switch
        {
            AppTheme.Light => "Themes/Light.axaml",
            AppTheme.Dark => "Themes/Dark.axaml",
            _ => "Themes/Light.axaml"
        };

        var themeDict = new ResourceInclude(new Uri("resm:Styles?assembly=RFD/Themes"))
        {
            Source = new Uri($"avares://RFD/{themePath}")
        };

        app.Resources.MergedDictionaries.Add(themeDict);

        CurrentTheme = theme;
    }
}