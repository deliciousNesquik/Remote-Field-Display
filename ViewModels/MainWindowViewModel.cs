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
    public ParametersSectionViewModel ParametersSectionViewModel { get; }
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
    
    private bool _isFirstCellVisible = true;
    
    public bool IsFirstCellVisible
    {
        get => _isFirstCellVisible;
        set
        {
            _isFirstCellVisible = value;
            OnPropertyChanged();
        }
    }
    private bool _isSecondCellVisible = true;
    public bool IsSecondCellVisible
    {
        get => _isSecondCellVisible;
        set
        {
            _isSecondCellVisible = value;
            OnPropertyChanged();
        }
    }
    private bool _isThirdCellVisible = true;
    public bool IsThirdCellVisible
    {
        get => _isThirdCellVisible;
        set
        {
            _isThirdCellVisible = value;
            OnPropertyChanged();
        }
    }
    private bool _isFourCellVisible = true;
    public bool IsFourCellVisible
    {
        get => _isFourCellVisible;
        set
        {
            _isFourCellVisible = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Переменные: Команды основного меню

    public ICommand OpenAutomaticConnectingCommand { get; }
    public ICommand OpenManualConnectingCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand SettingsCommand { get; }
    public ICommand AboutCommand { get; }

    #endregion

    #region Переменные: Параметры соединения с сервером
    private string _ipAddress;
    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            _ipAddress = value;
            OnPropertyChanged();
        }
    }

    private bool _connectionStatus;
    public bool ConnectionStatus
    {
        get => _connectionStatus;
        set
        {
            _connectionStatus = value;
            OnPropertyChanged();
        }
    }

    #endregion
    
    public MainWindowViewModel()
    {
        _currentUserControl = new();
        _ipAddress = "127.0.0.1";
        
        
        IsFirstCellVisible = true;
        IsSecondCellVisible = true;
        IsThirdCellVisible = true;
        IsFourCellVisible = true;
        TargetSectionViewModel = new TargetSectionViewModel(_windowService);
        ParametersSectionViewModel = new ParametersSectionViewModel(_windowService);
        InformationSectionViewModel = new InformationSectionViewModel(_windowService);
        StatusSectionViewModel = new StatusSectionViewModel(_windowService);
        
        FirstCell = new TargetSection()
        {
            DataContext = TargetSectionViewModel
        };
        SecondCell = new ParametersSection()
        {
            DataContext = ParametersSectionViewModel
        };
        ThirdCell = new InformationSection()
        {
            DataContext = InformationSectionViewModel
        };
        FourCell = new StatusSection()
        {
            DataContext = StatusSectionViewModel
        };

        //Использовать только эти два методы для создания сектора и его очистки
        //TargetSectionViewModel.SetSector(17.5, 37.5);
        //TargetSectionViewModel.SetSectorColor(Brush.Parse("#2B0068FF"));
        //TargetSectionViewModel.ClearSector();
        
        //InformationSectionViewModel.AddInfoBox(new InfoBox("Высота блока", "-", "м"));
        //InformationSectionViewModel.AddInfoBox(new InfoBox("Высота блока", "-", "м"));
        //InformationSectionViewModel.ClearInfoBox();
        
        //StatusSectionViewModel.AddStatusBox(new StatusBox("Насосы", true));
        //StatusSectionViewModel.AddStatusBox(new StatusBox("Забой", true));
        //StatusSectionViewModel.AddStatusBox(new StatusBox("Бурение", true));
        
        ParametersSectionViewModel.MagneticDeclination = 10.0;
        ParametersSectionViewModel.ToolfaceOffset = 12.0;
        ParametersSectionViewModel.Angle = 10.0;
        ParametersSectionViewModel.TimeStamp = DateTime.Now;
        ParametersSectionViewModel.ToolfaceType = "Нет данных";
        
        //Команды основного меню
        OpenAutomaticConnectingCommand = new RelayCommand(OpenAutomaticConnecting, () => !IsModalWindowOpen);
        OpenManualConnectingCommand = new RelayCommand(OpenManualConnecting, () => !IsModalWindowOpen);
        DisconnectCommand = new RelayCommand(Disconnect, () => ConnectionStatus);
        SettingsCommand = new RelayCommand(SwitchTheme, () => true);
        AboutCommand = new RelayCommand(OpenAbout, () => !IsModalWindowOpen);
    }

    private void SwitchTheme()
    {
        switch (App.Instance.ActualThemeVariant.Key.ToString())
        {
            case "Dark":
                App.Instance.Styles.Clear();
                App.Instance.Styles.Add(new FluentTheme());  // Принудительное обновление темы
                App.Instance.RequestedThemeVariant = ThemeVariant.Light;
                break;
            case "Light":
                
                App.Instance.Styles.Clear();
                App.Instance.Styles.Add(new FluentTheme());  // Принудительное обновление темы
                App.Instance.RequestedThemeVariant = ThemeVariant.Dark;
                break;
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
    
    public void UpdateConnecting(App? model)
    {
        switch (model)
        {
            //Проверка соединения с сервером
            case { IsConnected: true }:
                IpAddress = model.Address;
                ConnectionStatus = model.IsConnected;
                break;
            case { IsConnected: false }:
                IpAddress = model.Address;
                ConnectionStatus = model.IsConnected;
            
                //Очистка от данных все секции
                //Мишень - установка в начальное положение
                //Информационный блок - удаление всех данных
                //Статусный блок - удаление всех статусов
                TargetSectionViewModel.SetSector(-45, 45);
                InformationSectionViewModel.ClearInfoBox();
                ParametersSectionViewModel.MagneticDeclination = 0.0;
                ParametersSectionViewModel.ToolfaceOffset = 0.0;
                ParametersSectionViewModel.ToolfaceType = "Нет данных";
                ParametersSectionViewModel.TimeStamp = DateTime.Now;
                ParametersSectionViewModel.Angle = 0.0;
                break;
        }
    }
    #endregion
    
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}