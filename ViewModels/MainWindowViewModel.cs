using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using RFD.Interfaces;
using RFD.Services;
using RFD.UserControls;

namespace RFD.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly WindowService _windowService = new();
    private readonly ILoggerService _logger;
    private readonly IConnectionService _connectionService;
    
    public MainWindowViewModel(
        ILoggerService logger,
        IConnectionService connectionService)
    {
        _logger = logger;
        _connectionService = connectionService;
        
        _currentUserControl = new UserControl();
        _displayAddress = "Определение...";
        _disconnectTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _disconnectTimer.Tick += OnDisconnectTimerTick;

        TargetSectionViewModel = new TargetSectionViewModel(_windowService);
        InformationSectionViewModel = new InformationSectionViewModel(_windowService);
        StatusSectionViewModel = new StatusSectionViewModel(_windowService);

        FirstCell = new TargetSection { DataContext = TargetSectionViewModel };
        ThirdCell = new InformationSection { DataContext = InformationSectionViewModel };
        FourCell = new StatusSection { DataContext = StatusSectionViewModel };

        OpenAutomaticConnectingCommand =
            new RelayCommand(OpenAutomaticConnecting, () => !IsModalWindowOpen && !DisplayIsConnected);
        OpenManualConnectingCommand =
            new RelayCommand(OpenManualConnecting, () => !IsModalWindowOpen && !DisplayIsConnected);
        DisconnectCommand = new RelayCommand(Disconnect, () => DisplayIsConnected);

        SettingsCommand = new RelayCommand(() =>
                ThemeManager.ApplyTheme(ThemeManager.CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark),
            () => true);

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

    public TargetSectionViewModel TargetSectionViewModel { get; }
    public InformationSectionViewModel InformationSectionViewModel { get; }
    public StatusSectionViewModel StatusSectionViewModel { get; }

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

    #region Переменные: Параметры соединения с сервером

    private string _displayAddress;

    public string DisplayAddress
    {
        get => _displayAddress;
        set
        {
            if (_displayAddress != value)
            {
                _displayAddress = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _displayIsConnected;

    public bool DisplayIsConnected
    {
        get => _displayIsConnected;
        set
        {
            if (_displayIsConnected != value)
            {
                _displayIsConnected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayAddress));
            }
        }
    }

    private readonly DispatcherTimer _disconnectTimer;

    #endregion

    #region Методы: Методы для открытия окон соединения, запрос на разрыв соединения, проверка соединения

    private void OpenManualConnecting()
    {
        ManualConnectionDialogViewModel manualConnectionDialogViewModel = new();
        CurrentUserControl = new ManualConnectionDialog
        {
            DataContext = manualConnectionDialogViewModel
        };
        IsManualConnectingOpen = true;

        manualConnectionDialogViewModel.ConnectionAttempt += ip =>
        {
            manualConnectionDialogViewModel.ConnectionStatus.Invoke(_connectionService.ConnectAsync(ip).Result);
        };
        manualConnectionDialogViewModel.CloseDialog += () =>
        {
            CurrentUserControl = new UserControl();
            IsManualConnectingOpen = false;
        };
    }

    public void OpenAutomaticConnecting()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        AutomaticConnectionDialogViewModel automaticConnectionDialogViewModel = new();
        CurrentUserControl = new AutomaticConnectingDialog
        {
            DataContext = automaticConnectionDialogViewModel
        };
        IsAutomaticConnectingOpen = true;

        automaticConnectionDialogViewModel.UserCloseDialog += () =>
        {
            CurrentUserControl = new UserControl();
            IsAutomaticConnectingOpen = false;
            cancellationTokenSource.Cancel();
        };

        automaticConnectionDialogViewModel.CloseDialog += () =>
        {
            CurrentUserControl = new UserControl();
            IsAutomaticConnectingOpen = false;
        };

        Task.Run(async () =>
        {
            try
            {
                var isConnected = _connectionService.AutoConnectAsync().Result;

                // Если подключение успешно
                if (isConnected)
                {
                    automaticConnectionDialogViewModel.ConnectionStatus.Invoke(true);
                    Console.WriteLine($"[{DateTime.Now}] - [Connection to the server is successful]");
                }
                else
                {
                    automaticConnectionDialogViewModel.ConnectionStatus.Invoke(false);
                    Console.WriteLine($"[{DateTime.Now}] - [Couldn't connect to the server]");
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"[{DateTime.Now}] - [Connection canceled]");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{DateTime.Now}] - [Connection error: {e.Message}]");
            }
            finally
            {
                // В любом случае закрываем окно подключения
                await cancellationTokenSource.CancelAsync();
            }
        }, cancellationTokenSource.Token);
    }

    private void Disconnect()
    {
        
    }

    private void UpdateConnectionStatus(string address, bool connected)
    {
        if (connected)
        {
            // Если восстановили соединение - сразу обновляем
            _disconnectTimer.Stop();
            DisplayIsConnected = true;
            DisplayAddress = address;
        }
        else if (DisplayIsConnected)
        {
            // Если было подключение и потеряли - запускаем таймер
            if (!_disconnectTimer.IsEnabled) _disconnectTimer.Start();
        }
    }

    private void OnDisconnectTimerTick(object sender, EventArgs e)
    {
        _disconnectTimer.Stop();
        DisplayIsConnected = false;
        DisplayAddress = "Нет подключения";
    }
    
    public void OnConnectionStateChanged(string address = "Не найдено", bool connected = false)
    {
        UpdateConnectionStatus(address, connected);
    }

    #endregion
}