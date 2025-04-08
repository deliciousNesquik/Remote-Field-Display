using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using RFD.Models;
using RFD.UserControls;
using RFD.Services;

namespace RFD.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly WindowService _windowService = new WindowService();
    
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
        set {
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
    /// <summary> Принимает любой UserControl и отобразит его в левом нижнем углу </summary>
    public ContentControl SecondCell { get; set; }
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
    private DispatcherTimer _disconnectTimer;

    #endregion
    
    public MainWindowViewModel()
    {
        _currentUserControl = new UserControl();
        DisplayAddress = "Определение...";
        _disconnectTimer = new DispatcherTimer();
        _disconnectTimer.Interval = TimeSpan.FromSeconds(2);
        _disconnectTimer.Tick += OnDisconnectTimerTick;
        
        TargetSectionViewModel = new TargetSectionViewModel(_windowService); 
        InformationSectionViewModel = new InformationSectionViewModel(_windowService);
        StatusSectionViewModel = new StatusSectionViewModel(_windowService);
        
        FirstCell = new TargetSection() { DataContext = TargetSectionViewModel }; 
        ThirdCell = new InformationSection() { DataContext = InformationSectionViewModel };
        FourCell = new StatusSection() { DataContext = StatusSectionViewModel };
        
        OpenAutomaticConnectingCommand = new RelayCommand(OpenAutomaticConnecting, () => !IsModalWindowOpen && !DisplayIsConnected);
        OpenManualConnectingCommand = new RelayCommand(OpenManualConnecting, () => !IsModalWindowOpen && !DisplayIsConnected);
        DisconnectCommand = new RelayCommand(Disconnect, () => DisplayIsConnected);
        
        // Для переключения темы без параметра
        SettingsCommand = new RelayCommand(() => SwitchTheme(), () => true);
        AboutCommand = new RelayCommand(OpenAbout, () => !IsModalWindowOpen);
    }
    public void SwitchTheme(string? theme = null)
    {
        ThemeVariant newTheme;
    
        // Если тема указана явно
        if (!string.IsNullOrEmpty(theme))
        {
            newTheme = theme.Equals("Dark", StringComparison.OrdinalIgnoreCase) 
                ? ThemeVariant.Dark 
                : ThemeVariant.Light;
        }
        // Если нужно переключить на противоположную (вызов без параметра)
        else
        {
            newTheme = App.Instance.ActualThemeVariant == ThemeVariant.Dark 
                ? ThemeVariant.Light 
                : ThemeVariant.Dark;
        }

        App.Instance.RequestedThemeVariant = newTheme;
    
        // Принудительное обновление ресурсов (если требуется)
        if (App.Instance.Styles[0] is FluentTheme fluentTheme)
        {
            App.Instance.Styles[0] = new FluentTheme();
        }
    }

    private void OpenAbout()
    {
        AboutViewModel aboutViewModel = new();
        CurrentUserControl = new AboutDialog()
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
    
    #region Методы: Методы для открытия окон соединения, запрос на разрыв соединения, проверка соединения
    private void OpenManualConnecting()
    {
        ManualConnectionDialogViewModel manualConnectionDialogViewModel = new();
        CurrentUserControl = new ManualConnectionDialog()
        {
            DataContext = manualConnectionDialogViewModel
        };
        IsManualConnectingOpen = true;

        manualConnectionDialogViewModel.ConnectionAttempt += ip =>
        {
            manualConnectionDialogViewModel.ConnectionStatus.Invoke(App.Instance.Connect(ip));
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
        CurrentUserControl = new AutomaticConnectingDialog()
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
                bool isConnected = await App.Instance.AutoConnect();

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
        App.Instance.Disconnect();
    }
    
    private void UpdateConnectionStatus()
    {
        var isActuallyConnected = App.Instance.Client.Connected;
    
        if (isActuallyConnected)
        {
            // Если восстановили соединение - сразу обновляем
            _disconnectTimer.Stop();
            DisplayIsConnected = true;
            DisplayAddress = App.Instance.Client.Address.ToString();
        }
        else if (DisplayIsConnected)
        {
            // Если было подключение и потеряли - запускаем таймер
            if (!_disconnectTimer.IsEnabled)
            {
                _disconnectTimer.Start();
            }
        }
    }

    private void OnDisconnectTimerTick(object sender, EventArgs e)
    {
        _disconnectTimer.Stop();
        DisplayIsConnected = false;
        DisplayAddress = "Нет подключения";
    }

// Вызывайте этот метод при любых изменениях подключения
    public void OnConnectionStateChanged()
    {
        UpdateConnectionStatus();
    }
    #endregion
    
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}