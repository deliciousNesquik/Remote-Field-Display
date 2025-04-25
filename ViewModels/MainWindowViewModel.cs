using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using NPFGEO.LWD.Net;
using RFD.Core;
using RFD.Interfaces;
using RFD.Services;
using RFD.UserControls;

namespace RFD.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ILoggerService _logger;
    private readonly IConnectionService _connectionService;
    
    public bool UseDefaultMenu { get; set; } = PlatformUtils.IsWindows || PlatformUtils.IsLinux;
    public ConnectStatusViewModel? ConnectStatusViewModel { get; set; }
    
    public MainWindowViewModel()
    {
        _logger = new NLoggerService();
        _connectionService = new ConnectionService(new Client(), _logger);

        _currentUserControl = new UserControl();
        _isModalWindowOpen = false;

        ConnectStatusViewModel = new ConnectStatusViewModel();
        
        FirstCell = new TargetSection { DataContext = TargetSectionViewModel };
        ThirdCell = new InformationSection { DataContext = InformationSectionViewModel };
        FourCell = new StatusSection { DataContext = StatusSectionViewModel };
        
        
        OpenAutomaticConnectingCommand = new RelayCommand(OpenAutomaticConnecting, () => !IsModalWindowOpen && !ConnectStatusViewModel.Status);
        OpenManualConnectingCommand = new RelayCommand(OpenManualConnecting, () => !IsModalWindowOpen && !ConnectStatusViewModel.Status);
        DisconnectCommand = new RelayCommand(Disconnect, () => ConnectStatusViewModel.Status);
        SettingsCommand = new RelayCommand(() => ThemeManager.ApplyTheme(ThemeManager.CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark), () => true);
        AboutCommand = new RelayCommand(OpenAbout, () => !IsModalWindowOpen);
    }
    
    public MainWindowViewModel(
        ILoggerService logger,
        IConnectionService connectionService)
    {
        _logger = logger;
        _connectionService = connectionService;
        
        _currentUserControl = new UserControl();
        _isModalWindowOpen = false;
        
        ConnectStatusViewModel = new ConnectStatusViewModel();

        FirstCell = new TargetSection { DataContext = TargetSectionViewModel };
        ThirdCell = new InformationSection { DataContext = InformationSectionViewModel };
        FourCell = new StatusSection { DataContext = StatusSectionViewModel };

        OpenAutomaticConnectingCommand = new RelayCommand(OpenAutomaticConnecting, () => !IsModalWindowOpen && !ConnectStatusViewModel.Status);
        OpenManualConnectingCommand = new RelayCommand(OpenManualConnecting, () => !IsModalWindowOpen && !ConnectStatusViewModel.Status);
        DisconnectCommand = new RelayCommand(Disconnect, () => ConnectStatusViewModel.Status);
        SettingsCommand = new RelayCommand(() => ThemeManager.ApplyTheme(ThemeManager.CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark), () => true);
        AboutCommand = new RelayCommand(OpenAbout, () => !IsModalWindowOpen);
    }


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

    public TargetSectionViewModel TargetSectionViewModel { get; } = new TargetSectionViewModel(new WindowService());
    public InformationSectionViewModel InformationSectionViewModel { get; } = new InformationSectionViewModel(new WindowService());
    public StatusSectionViewModel StatusSectionViewModel { get; } = new StatusSectionViewModel(new WindowService());

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

    private bool _isManualConnectingOpen;

    public bool IsManualConnectingOpen
    {
        get => _isManualConnectingOpen;
        set
        {
            _isManualConnectingOpen = value;
            IsModalWindowOpen = value;
            OnPropertyChanged();
        }
    }

    private bool _isAutomaticConnectingOpen;

    public bool IsAutomaticConnectingOpen
    {
        get => _isAutomaticConnectingOpen;
        set
        {
            _isAutomaticConnectingOpen = value;
            IsModalWindowOpen = value;
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
        IsAutomaticConnectingOpen = false;
        IsManualConnectingOpen = false;
    }
    
    private void OpenManualConnecting()
    {
        IsManualConnectingOpen = true;
        var manualConnectionDialogViewModel = new ManualConnectionDialogViewModel(_connectionService, _logger);
        CurrentUserControl = new ManualConnectionDialog() { DataContext = manualConnectionDialogViewModel };
        manualConnectionDialogViewModel.DialogClose += HideConnectionDialog;
    }

    private void OpenAutomaticConnecting()
    {
        IsAutomaticConnectingOpen = true;
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