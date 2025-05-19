using System.Reactive;
using Avalonia;
using Avalonia.Media;
using ReactiveUI;
using RFD.Interfaces;
using RFD.Services;

namespace RFD.ViewModels;

public class SettingsViewModel: ViewModelBase
{
    public Action? CloseDialog;

    private bool _saveData = true;
    private string _brandColor = "";
    private bool _darkTheme = ThemeManager.CurrentTheme == AppTheme.Dark;
    
    public bool SaveData
    {
        get => _saveData;
        set
        {
            SettingsApplication.SaveData = value;
            this.RaiseAndSetIfChanged(ref _saveData, value);
        }
    }

    public string BrandColor
    {
        get => _brandColor;
        set
        {
            if (Color.TryParse(value, out Color _))
            {
                this.RaiseAndSetIfChanged(ref _brandColor, value);
                ChangeBrandColor(value);
            }
        }
    }

    public bool DarkTheme
    {
        get => _darkTheme;
        set
        {
            this.RaiseAndSetIfChanged(ref _darkTheme, value);
            ThemeManager.ApplyTheme(value ? AppTheme.Dark : AppTheme.Light);
        }
    }

    public int SelectLanguage
    {
        get
        {
            return LocalizationManager.CurrentLocalization switch
            {
                Localization.RU => 0,
                Localization.EN => 1,
                Localization.BASH => 2,
                _ => 0
            };
        }
        set
        {
            SettingsApplication.Language = value;
            switch (value)
            {
                case 0:
                    LocalizationManager.ApplyLocalization(Localization.RU);
                    SettingsApplication.LanguageLocalization = Localization.RU;
                    break;
                case 1:
                    LocalizationManager.ApplyLocalization(Localization.EN);
                    SettingsApplication.LanguageLocalization = Localization.EN;
                    break;
                case 2:
                    LocalizationManager.ApplyLocalization(Localization.BASH);
                    SettingsApplication.LanguageLocalization = Localization.BASH;
                    break;
                default:
                    LocalizationManager.ApplyLocalization(Localization.RU);
                    SettingsApplication.LanguageLocalization = Localization.RU;
                    break; 
            }
        }
    }
    
    public static void ChangeBrandColor(string hexColor)
    {
        if (Application.Current?.Resources.TryGetResource("FocusingColor", Application.Current.ActualThemeVariant, out _) == true)
        {
            Application.Current.Resources["FocusingColor"] = Color.Parse(hexColor);
            SettingsApplication.BrandColor = hexColor;
        }
    }
    
    public SettingsViewModel()
    {
        BrandColor = SettingsApplication.BrandColor;
        SelectLanguage = SettingsApplication.Language;
        SaveData = SettingsApplication.SaveData;
        
        Color1Command = ReactiveCommand.Create((() => ChangeBrandColor("#3e66ad")));
        Color2Command = ReactiveCommand.Create((() => ChangeBrandColor("#50ADA8")));
        Color3Command = ReactiveCommand.Create((() => ChangeBrandColor("#31AD2D")));
        Color4Command = ReactiveCommand.Create((() => ChangeBrandColor("#ADAA41")));
        Color5Command = ReactiveCommand.Create((() => ChangeBrandColor("#AD4A3F")));
        CloseCommand = ReactiveCommand.Create(Close);
    }

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
    public ReactiveCommand<Unit, Unit> Color1Command { get; }
    public ReactiveCommand<Unit, Unit> Color2Command { get; }
    public ReactiveCommand<Unit, Unit> Color3Command { get; }
    public ReactiveCommand<Unit, Unit> Color4Command { get; }
    public ReactiveCommand<Unit, Unit> Color5Command { get; }

    private void Close()
    {
        SettingsApplication.Save();
        CloseDialog?.Invoke();
    }
}