using Avalonia;
using Avalonia.Markup.Xaml.Styling;

namespace RFD.Services;

public enum Localization
{
    RU,
    EN,
    BASH,
    CH,
}

public class LocalizationManager
{
    public static Localization CurrentLocalization { get; private set; } = SettingsApplication.LanguageLocalization;

    public static void ApplyLocalization(Localization localization)
    {
        var app = Application.Current;
        if (app is null)
            return;

        var existingTheme = app.Resources.MergedDictionaries
            .OfType<ResourceInclude>()
            .FirstOrDefault(x => x.Source?.OriginalString.Contains("Localization") == true);

        if (existingTheme != null)
            app.Resources.MergedDictionaries.Remove(existingTheme);

        var themePath = localization switch
        {
            Localization.RU => "Localizations/RU-ru.axaml",
            Localization.EN => "Localizations/EN-en.axaml",
            Localization.BASH => "Localizations/BASH-bash.axaml",
            Localization.CH => "Localizations/CH-ch.axaml",
            _ => "Localizations/RU-ru.axaml"
        };

        var themeDict = new ResourceInclude(new Uri("resm:Styles?assembly=RFD/Localizations"))
        {
            Source = new Uri($"avares://RFD/{themePath}")
        };

        app.Resources.MergedDictionaries.Add(themeDict);

        CurrentLocalization = localization;
    }
}