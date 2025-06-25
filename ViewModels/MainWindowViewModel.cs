using System.Windows.Input;
using Avalonia.Controls;
using NPFGEO.LWD.Net;
using ReactiveUI;
using RFD.Core;
using RFD.Interfaces;
using RFD.Services;
using RFD.UserControls;

namespace RFD.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IConnectionService _connectionService;
    private readonly ILoggerService _logger;
    
    public bool UseDefaultMenu { get; set; } = PlatformUtils.IsWindows || PlatformUtils.IsLinux;
    public bool UseNativeMenu { get; set; } = PlatformUtils.IsMacOS;
    
    public ICommand OpenAutomaticConnectingCommand { get; }
    public ICommand OpenManualConnectingCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand SettingsCommand { get; }
    public ICommand AboutCommand { get; }
    
    public List<ContentControl> SectionsContentControl { get; set; } = [];
    public TargetSectionViewModel TargetSectionViewModel { get; } = new(new WindowService());
    public InformationSectionViewModel InformationSectionViewModel { get; } = new(new WindowService());
    public StatusSectionViewModel StatusSectionViewModel { get; } = new(new WindowService());
    public ConnectStatusViewModel ConnectStatusViewModel { get; set; } = new();
    
    public MainWindowViewModel()
    {
        _logger = new NLoggerService();
        _connectionService = new ConnectionService(new Client(), _logger);

        _currentModalWindow = new UserControl();
        _isModalWindowOpen = false;

        UseDefaultMenu = true;
        UseNativeMenu = false;

        SectionsContentControl.Add(new TargetSection { DataContext = TargetSectionViewModel });
        SectionsContentControl.Add(new InformationSection { DataContext = InformationSectionViewModel });
        SectionsContentControl.Add(new StatusSection { DataContext = StatusSectionViewModel });
        
        OpenAutomaticConnectingCommand = ReactiveCommand.Create(OpenAutomaticConnecting);
        OpenManualConnectingCommand = ReactiveCommand.Create(OpenManualConnecting);
        DisconnectCommand = ReactiveCommand.Create(Disconnect);
        SettingsCommand = ReactiveCommand.Create(OpenSettings);
        AboutCommand = ReactiveCommand.Create(OpenAbout);
    }

    public MainWindowViewModel(ILoggerService logger, IConnectionService connectionService)
    {
        _logger = logger;
        _connectionService = connectionService;

        _currentModalWindow = new UserControl();
        _isModalWindowOpen = false;

        SectionsContentControl.Add(new TargetSection { DataContext = TargetSectionViewModel });
        SectionsContentControl.Add(new InformationSection { DataContext = InformationSectionViewModel });
        SectionsContentControl.Add(new StatusSection { DataContext = StatusSectionViewModel });
        
        OpenAutomaticConnectingCommand = ReactiveCommand.Create(OpenAutomaticConnecting);
        OpenManualConnectingCommand = ReactiveCommand.Create(OpenManualConnecting);
        DisconnectCommand = ReactiveCommand.Create(Disconnect);
        SettingsCommand = ReactiveCommand.Create(OpenSettings);
        AboutCommand = ReactiveCommand.Create(OpenAbout);
    }
    
    #region Работа с модульными окнами.
    private UserControl _currentModalWindow; // Текущее открытое модальное окно.
    private bool _isModalWindowOpen;         // Состояние модального окна.
    private double _blurRadius;              // Блюр радиус, чтобы размывать задний фон при открытом окне.

    public UserControl CurrentModalWindow
    {
        get => _currentModalWindow;
        set => this.RaiseAndSetIfChanged(ref _currentModalWindow, value);
    }

    public bool IsModalWindowOpen
    {
        get => _isModalWindowOpen;
        set
        {
            BlurRadius = value ? 10 : 0;
            this.RaiseAndSetIfChanged(ref _isModalWindowOpen, value);
        }
    }

    public double BlurRadius
    {
        get => _blurRadius;
        set => this.RaiseAndSetIfChanged(ref _blurRadius, value);
    }
    
    /// <summary> Скрытие модульного окна. </summary>
    private void HideConnectionDialog()
    {
        _logger.Info("Cкрытие модульного окна");
        
        CurrentModalWindow = new UserControl();
        IsModalWindowOpen = false;
    }
    
    /// <summary> Открытие модульного окна "О приложении". </summary>
    private void OpenAbout()
    {
        _logger.Info("Открытие модульного окна \"О приложении\"");
        
        IsModalWindowOpen = true;
        var aboutViewModel = new AboutViewModel();
        CurrentModalWindow = new AboutDialog { DataContext = aboutViewModel };
        aboutViewModel.CloseDialog += HideConnectionDialog;
    }
    
    /// <summary> Открытие модульного окна настроек приложения. </summary>
    private void OpenSettings()
    {
        _logger.Info("Открытие модульного окна \"Настройки\"");
        
        IsModalWindowOpen = true;
        var settingsViewModel = new SettingsViewModel();
        CurrentModalWindow = new SettingsView() { DataContext = settingsViewModel };
        settingsViewModel.CloseDialog += HideConnectionDialog;
    }

    /// <summary> Открытие модульного окна ручного подключения. </summary>
    private void OpenManualConnecting()
    {
        _logger.Info("Открытие модульного окна \"Ручное подключение\"");
        
        IsModalWindowOpen = true;
        var manualConnectionDialogViewModel = new ManualConnectionDialogViewModel(_connectionService, _logger);
        CurrentModalWindow = new ManualConnectionDialog { DataContext = manualConnectionDialogViewModel };
        manualConnectionDialogViewModel.DialogClose += HideConnectionDialog;
    }

    /// <summary> Открытие модульного окна автоматического подключения. </summary>
    private void OpenAutomaticConnecting()
    {
        _logger.Info("Открытие модульного окна \"Автоматическое подключение\"");

        IsModalWindowOpen = true;
        var automaticConnectingDialogViewModel = new AutomaticConnectionDialogViewModel(_connectionService, _logger);
        CurrentModalWindow = new AutomaticConnectingDialog { DataContext = automaticConnectingDialogViewModel };
        automaticConnectingDialogViewModel.DialogClose += HideConnectionDialog;
    }
    
    /// <summary> Команда для отключения от сервера. </summary>
    private void Disconnect()
    {
        _logger.Info("Отключение от сервера.");
        
        _logger.Info("Проверка пункта сохранение данных.");
        if (!SettingsApplication.SaveData)
        {
            _logger.Info("Сохранение данных выключено.");
            
            _logger.Info("Очистка секций с данными");
            InformationSectionViewModel.ClearInfoBox();
            StatusSectionViewModel.ClearStatusBox();
            TargetSectionViewModel.SetDefaultTarget();
        }
        else
        {
            _logger.Info("Сохранение данных включено.");
        }

        try { _connectionService.Disconnect(); }
        catch(Exception ex) { _logger.Error($"Ошибка при попытке отключится от сервера: {ex}"); }
    }
    #endregion

}