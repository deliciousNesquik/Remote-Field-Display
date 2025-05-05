using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls;
using NPFGEO.LWD.Net;
using ReactiveUI;
using RFD.Core;
using RFD.Interfaces;
using RFD.Services;
using RFD.UserControls;

namespace RFD.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
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


        OpenAutomaticConnectingCommand = ReactiveCommand.Create(OpenAutomaticConnecting);

        OpenManualConnectingCommand = ReactiveCommand.Create(OpenManualConnecting);

        DisconnectCommand = ReactiveCommand.Create(Disconnect);

        SettingsCommand = ReactiveCommand.Create(
            () => ThemeManager.ApplyTheme(ThemeManager.CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark)
        );

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
        
        OpenAutomaticConnectingCommand = ReactiveCommand.Create(OpenAutomaticConnecting);
        OpenManualConnectingCommand = ReactiveCommand.Create(OpenManualConnecting);
        DisconnectCommand = ReactiveCommand.Create(Disconnect);
        SettingsCommand = ReactiveCommand.Create(() => ThemeManager.ApplyTheme(ThemeManager.CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark));
        AboutCommand = ReactiveCommand.Create(OpenAbout);
    }

    public bool UseDefaultMenu { get; set; } = PlatformUtils.IsWindows || PlatformUtils.IsLinux;
    public bool UseNativeMenu { get; set; } = PlatformUtils.IsMacOS;
    public ConnectStatusViewModel? ConnectStatusViewModel { get; set; } = new ConnectStatusViewModel();


    public event PropertyChangedEventHandler? PropertyChanged;

    private void OpenAbout()
    {
        AboutViewModel aboutViewModel = new();
        CurrentUserControl = new AboutDialog
        {
            DataContext = aboutViewModel
        };
        IsModalWindowOpen = true;

        aboutViewModel.CloseDialog += () =>
        {
            IsModalWindowOpen = false;
            CurrentUserControl = new UserControl();
        };
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

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
        set
        {
            _currentUserControl = value;
            OnPropertyChanged();
        }
    }

    private bool _isModalWindowOpen;

    public bool IsModalWindowOpen
    {
        get => _isModalWindowOpen;
        set
        {
            _isModalWindowOpen = value;
            BlurRadius = value ? 10 : 0;
            OnPropertyChanged();
        }
    }

    private double _blurRadius;

    public double BlurRadius
    {
        get => _blurRadius;
        set
        {
            _blurRadius = value;
            OnPropertyChanged();
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

    private void OpenManualConnecting()
    {
        IsModalWindowOpen = true;
        var manualConnectionDialogViewModel = new ManualConnectionDialogViewModel(_connectionService, _logger);
        CurrentUserControl = new ManualConnectionDialog { DataContext = manualConnectionDialogViewModel };
        manualConnectionDialogViewModel.DialogClose += HideConnectionDialog;
    }

    private void OpenAutomaticConnecting()
    {
        IsModalWindowOpen = true;
        var automaticConnectingDialogViewModel = new AutomaticConnectionDialogViewModel(_connectionService, _logger);
        CurrentUserControl = new AutomaticConnectingDialog { DataContext = automaticConnectingDialogViewModel };
        automaticConnectingDialogViewModel.DialogClose += HideConnectionDialog;
    }

    private void Disconnect()
    {
        _connectionService.Disconnect();
    }

    #endregion
}