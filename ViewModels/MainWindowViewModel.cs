using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using NPFGEO.LWD.Net;
using ReactiveUI;
using RFD.Core;
using RFD.Interfaces;
using RFD.Models;
using RFD.Services;
using RFD.UserControls;

namespace RFD.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IConnectionService _connectionService;
    private readonly ILoggerService _logger;
    
    public MainWindowViewModel()
    {
        _logger = new NLoggerService();
        _connectionService = new ConnectionService(new Client(), _logger);

        _currentUserControl = new UserControl();
        _isModalWindowOpen = false;

        UseDefaultMenu = true;
        UseNativeMenu = false;

        FirstCell = new TargetSection { DataContext = TargetSectionViewModel };
        ThirdCell = new InformationSection { DataContext = InformationSectionViewModel };
        FourCell = new StatusSection { DataContext = StatusSectionViewModel };
        ConnectStatusViewModel = new ConnectStatusViewModel();
        
        OpenAutomaticConnectingCommand = ReactiveCommand.Create(OpenAutomaticConnecting);
        OpenManualConnectingCommand = ReactiveCommand.Create(OpenManualConnecting);
        DisconnectCommand = ReactiveCommand.Create(Disconnect);
        SettingsCommand = ReactiveCommand.Create(OpenSettings);
        AboutCommand = ReactiveCommand.Create(OpenAbout);
    }

    public MainWindowViewModel(
        ILoggerService logger,
        IConnectionService connectionService)
    {
        _logger = logger;
        _connectionService = connectionService;

        _currentUserControl = new UserControl();
        _isModalWindowOpen = false;

        FirstCell = new TargetSection { DataContext = TargetSectionViewModel };
        ThirdCell = new InformationSection { DataContext = InformationSectionViewModel };
        FourCell = new StatusSection { DataContext = StatusSectionViewModel };
        ConnectStatusViewModel = new ConnectStatusViewModel();
        
        OpenAutomaticConnectingCommand = ReactiveCommand.Create(OpenAutomaticConnecting);
        OpenManualConnectingCommand = ReactiveCommand.Create(OpenManualConnecting);
        DisconnectCommand = ReactiveCommand.Create(Disconnect);
        SettingsCommand = ReactiveCommand.Create(OpenSettings);
        AboutCommand = ReactiveCommand.Create(OpenAbout);
    }

    public bool UseDefaultMenu { get; set; } = PlatformUtils.IsWindows || PlatformUtils.IsLinux;
    public bool UseNativeMenu { get; set; } = PlatformUtils.IsMacOS;
    public ConnectStatusViewModel? ConnectStatusViewModel { get; set; }

    #region UserControl содержащие секции (Мишень, Параметры, Информация, Статусы)

    public TargetSectionViewModel TargetSectionViewModel { get; } = new(new WindowService());
    public InformationSectionViewModel InformationSectionViewModel { get; } = new(new WindowService());
    public StatusSectionViewModel StatusSectionViewModel { get; } = new(new WindowService());

    #endregion

    #region Переменные: Параметры отвечающие за работу модальных окон поверх главного окна

    private UserControl _currentUserControl;

    public UserControl CurrentUserControl
    {
        get => _currentUserControl;
        set => this.RaiseAndSetIfChanged(ref _currentUserControl, value);
    }

    private bool _isModalWindowOpen;

    public bool IsModalWindowOpen
    {
        get => _isModalWindowOpen;
        set
        {
            BlurRadius = value ? 10 : 0;
            this.RaiseAndSetIfChanged(ref _isModalWindowOpen, value);
        }
    }

    private double _blurRadius;

    public double BlurRadius
    {
        get => _blurRadius;
        set
        {
            _blurRadius = value;
            this.RaiseAndSetIfChanged(ref _blurRadius, value);
        }
    }

    #endregion

    #region Переменные: Параметры отвечающие за настройку окон внутри главного окна

    /// <summary> Принимает любой UserControl и отобразит его в левом верхнем углу </summary>
    public ContentControl FirstCell { get; set; }

    /// <summary> Принимает любой UserControl и отобразит его в правом верхнем углу </summary>
    public ContentControl ThirdCell { get; set; }

    /// <summary> Принимает любой UserControl и отобразит его в правом нижнем углу </summary>
    public ContentControl FourCell { get; set; }

    #endregion

    #region Переменные: Команды основного меню

    public ICommand OpenAutomaticConnectingCommand { get; }
    public ICommand OpenManualConnectingCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand SettingsCommand { get; }
    public ICommand AboutCommand { get; }

    #endregion

    #region Методы: Методы для открытия окон соединения, запрос на разрыв соединения, проверка соединения

    private void HideConnectionDialog()
    {
        CurrentUserControl = new UserControl();
        IsModalWindowOpen = false;
    }

    private void OpenAbout()
    {
        IsModalWindowOpen = true;
        var aboutViewModel = new AboutViewModel();
        CurrentUserControl = new AboutDialog { DataContext = aboutViewModel };
        aboutViewModel.CloseDialog += HideConnectionDialog;
    }
    
    public void OpenSettings()
    {
        IsModalWindowOpen = true;
        var settingsViewModel = new SettingsViewModel();
        CurrentUserControl = new SettingsView() { DataContext = settingsViewModel };
        settingsViewModel.CloseDialog += HideConnectionDialog;
    }
    
    public void OpenManualConnecting()
    {
        IsModalWindowOpen = true;
        var manualConnectionDialogViewModel = new ManualConnectionDialogViewModel(_connectionService, _logger);
        CurrentUserControl = new ManualConnectionDialog { DataContext = manualConnectionDialogViewModel };
        manualConnectionDialogViewModel.DialogClose += HideConnectionDialog;
    }

    public void OpenAutomaticConnecting()
    {
        IsModalWindowOpen = true;
        var automaticConnectingDialogViewModel = new AutomaticConnectionDialogViewModel(_connectionService, _logger);
        CurrentUserControl = new AutomaticConnectingDialog { DataContext = automaticConnectingDialogViewModel };
        automaticConnectingDialogViewModel.DialogClose += HideConnectionDialog;
    }

    public void Disconnect()
    {
        if (!SettingsApplication.SaveData)
        {
            _logger.Info("Сохранение настроек и данных отключено, очищаю интерфейс...");
            InformationSectionViewModel.ClearInfoBox();
            StatusSectionViewModel.ClearStatusBox();
            TargetSectionViewModel.SetDefaultTarget();
        }
        else
        {
            _logger.Info("Сохранение настроек и данных включено, отключаюсь от сервера...");
        }
        _connectionService.Disconnect();
    }

    #endregion
}