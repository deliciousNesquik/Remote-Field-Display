using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RFD.Models;
using RFD.UserControls;
using ReactiveUI;
using NPFGEO.LWD.Net;

namespace RFD.ViewModels;

public partial class MainWindowViewModel : INotifyPropertyChanged
{
    #region Переменные: Геофизические параметры
    public ObservableCollection<InfoBox> InfoBlockList { get; private set; }
    public ObservableCollection<StatusBox> InfoStatusList { get; private set; }
    public double MagneticDeclination { get; private set; } 
    public double ToolfaceOffset { get; private set; } 
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
        
    private bool _isModalWindowOpen = false;
    public bool IsModalWindowOpen
    {
        get => _isModalWindowOpen;
        set {
            _isModalWindowOpen = value;
            if (value)
            {
                BlurRadius = 10;
            }
            else
            {
                BlurRadius = 0;
            }
            OnPropertyChanged();
        }
    }

    private bool _isManualConnectingOpen = false;
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

    private bool _isAutomaticConnectingOpen = false;
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
        
    private double _blurRadius = 0;
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
    private bool _isTargetDisplayed;
    public bool IsTargetDisplayed
    {
        get => _isTargetDisplayed;
        set
        {
            _isTargetDisplayed = value;
            OnPropertyChanged();
        }
    }
    private bool _isInformationDisplayed;
    public bool IsInformationDisplayed
    {
        get => _isInformationDisplayed;
        set
        {
            _isInformationDisplayed = value;
            OnPropertyChanged();
        }
    }
    private bool _isParametersDisplayed;
    public bool IsParametersDisplayed
    {
        get => _isParametersDisplayed;
        set
        {
            _isParametersDisplayed = value;
            OnPropertyChanged();
        }
    }
    private bool _isStatusesDisplayed;
    public bool IsStatusesDisplayed
    {
        get => _isStatusesDisplayed;
        set
        {
            _isStatusesDisplayed = value;
            OnPropertyChanged();
        }
    }
    private bool _isConditionsDisplayed;
    public bool IsConditionsDisplayed
    {
        get => _isConditionsDisplayed;
        set
        {
            _isConditionsDisplayed = value;
            OnPropertyChanged();
        }
    }
    #endregion
    
    #region Переменные: View Models модальных окон
    public ManualConnectionDialogViewModel ManualConnectionDialogViewModel { get; set; }
    public AutomaticConnectionDialogViewModel AutomaticConnectionDialogViewModel { get; set; }
    #endregion

    #region Переменные: Команды основного меню

    public ICommand OpenAutomaticConnectingCommand { get; }
    public ICommand OpenManualConnectingCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand SettingsCommand { get; }
    public ICommand TargetVisibleCommand { get; }
    public ICommand InformationVisibleCommand { get; }
    public ICommand ParametersVisibleCommand { get; }
    public ICommand StatusesVisibleCommand { get; }
    public ICommand ConditionsVisibleCommand { get; }

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

    #region Переменные: Для связи между App.xaml.cs и текущим файлом
    private RFD.App Model => App.Current as RFD.App;
    #endregion
        
    public MainWindowViewModel() 
    {
        //Геофизические параметры заполнены для примера
        InfoBlockList = [
            new ("Высота блока", "-", "м"),
            new ("Глубина долота", "-", "м"),
            new ("Текущий забой", "-", "м"),
            new ("TVD", "-", "м"),
            new ("Расстояние до забоя", "-", "м"),
            new ("Rop средний", "-", "м/ч"),
            new ("Зенит", "-", "°"),
            new ("Азимут", "-", "°"),
        ];
        InfoStatusList = [
            new ("Клинья", false),
            new ("Насос", false),
            new ("Забой", false),
        ];
            
        MagneticDeclination = 0.00;
        ToolfaceOffset = 0.00;

        IsTargetDisplayed = true;
        IsInformationDisplayed = true;
        IsParametersDisplayed = true;
        IsStatusesDisplayed = true;
        IsConditionsDisplayed = true;
    
        App.ConnectionUpdated += UpdateConnecting;
        //App.SettingsUpdated += SetSettings;
            
        //Команды основного меню
        OpenAutomaticConnectingCommand = new RelayCommand(() => OpenAutomaticConnecting(), () => !IsModalWindowOpen);
        OpenManualConnectingCommand = new RelayCommand(() => OpenManualConnecting(), () => !IsModalWindowOpen);
        DisconnectCommand = new RelayCommand(() => Disconnect(), () => ConnectionStatus);
        TargetVisibleCommand = new RelayCommand(() =>
        {
            IsTargetDisplayed = !IsTargetDisplayed;
            Console.WriteLine(IsTargetDisplayed);
        });
        InformationVisibleCommand = new RelayCommand(() =>
        {
            IsInformationDisplayed = !IsInformationDisplayed;
            Console.WriteLine(InformationVisibleCommand);
        });
        ParametersVisibleCommand = new RelayCommand(() =>
        {
            IsParametersDisplayed = !IsParametersDisplayed;
            Console.WriteLine(IsParametersDisplayed);
        });
        StatusesVisibleCommand = new RelayCommand(() =>
        {
            IsStatusesDisplayed = !IsStatusesDisplayed;
            Console.WriteLine(IsStatusesDisplayed);
        });
        ConditionsVisibleCommand = new RelayCommand(() =>
        {
            IsConditionsDisplayed = !IsConditionsDisplayed;
            Console.WriteLine(IsConditionsDisplayed);
        });
    }

    #region Методы: Методы для открытия окон соединения с сервером

    /*Метод для открытия ручного окна соединения*/
    private void OpenManualConnecting()
    {
        ManualConnectionDialogViewModel = new ManualConnectionDialogViewModel();
        CurrentUserControl = new ManualConnectionDialog()
        {
            DataContext = ManualConnectionDialogViewModel
        };
        IsManualConnectingOpen = true;

        ManualConnectionDialogViewModel.ConnectionAttempt += ip =>
        {
            //TODO
            //Добавить метод для передачи, проверки IP-адреса и подключение по нему
            ManualConnectionDialogViewModel.ConnectionStatus?.Invoke(true);
        };
        ManualConnectionDialogViewModel.CloseDialog += () =>
        {
            IsManualConnectingOpen = false;
            UpdateConnecting();
            CurrentUserControl = null;
        };
    }
        
    /*Метод для открытия автоматического окна соединения*/
    public void OpenAutomaticConnecting()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        AutomaticConnectionDialogViewModel = new AutomaticConnectionDialogViewModel();
        CurrentUserControl = new AutomaticConnectingDialog()
        {
            DataContext = AutomaticConnectionDialogViewModel
        };
        IsAutomaticConnectingOpen = true;

        AutomaticConnectionDialogViewModel.UserCloseDialog += () =>
        {
            cancellationTokenSource.Cancel();
            IsAutomaticConnectingOpen = false;
            UpdateConnecting();
            CurrentUserControl = null;
        };

        AutomaticConnectionDialogViewModel.CloseDialog += () =>
        {
            IsAutomaticConnectingOpen = false;
            UpdateConnecting();
            CurrentUserControl = null;
        };
            
        // Имитация подключения с использованием Task.Run
        Task.Run(async () =>
        {
            try
            {
                // Имитация ожидания 2 секунды
                await Task.Delay(3000, cancellationTokenSource.Token);

                // Если задача не была отменена, сообщаем об успешном подключении
                AutomaticConnectionDialogViewModel.ConnectionStatus?.Invoke(true);
                Console.WriteLine("Connection was succsess");
            }
            catch (TaskCanceledException)
            {
                // Обрабатываем отмену задачи (ничего делать не нужно)
                Console.WriteLine("Connection was canceled.");
            }
        });
    }
    #endregion

    #region Методы: Методы для соединения с сервером
    public void Disconnect()
    {
        Console.WriteLine("User disconnect from server");
        Console.WriteLine("Info status list count items: " + InfoStatusList.Count);
        Console.WriteLine("Info block list count items: " + InfoBlockList.Count);
        InfoStatusList.Clear();
        InfoBlockList.Clear();
            
        Console.WriteLine("Info status list count items: " + InfoStatusList.Count);
        Console.WriteLine("Info block list count items: " + InfoBlockList.Count);
        /*if (Model != null)
        {
            Model.Disconnect();
        }*/
    }
    public void UpdateConnecting()
    {
        Console.WriteLine("Connection has updated: " + "{Model.CurrentIpAddress: " + Model.CurrentIpAddress +", Model.Connected: " + true + "}");
        IpAddress = Model.CurrentIpAddress;
        ConnectionStatus = true;
    }
    #endregion
        
    #region Методы для получения данных из Genesis LWD
    /*public void SetSettings(ReceiveSettingsEventArgs e)
    {
        Console.WriteLine("SetSettings ZOV");
        InfoBlockList.Clear();
        InfoStatusList.Clear();
        MagneticDeclination = 0.0;
        ToolfaceOffset = 0.0;
            
        MagneticDeclination = e.Settings.InfoParameters.MagneticDeclination;
        ToolfaceOffset = e.Settings.InfoParameters.ToolfaceOffset;
        //add 
        //booIndicators - красные индикаторы
            
        foreach (var flag in e.Settings.Statuses)
        {
            Console.WriteLine("statuses name" + flag.Name.ToString());
            InfoStatusList.Add(Convert(flag));
        }

        /*foreach (var flag in e.Settings.ParameterInfo)
        {
            Console.WriteLine("parameterInfo name " + flag.Name.ToString());
            Console.WriteLine("parameterInfo float " + flag.Float.ToString());
            Console.WriteLine("parameterInfo units " + flag.Units.ToString());
            InfoBlockList.Add(Convert(flag));
        }*/

        /*foreach (var param in e.Settings.Params)
        {
            Console.WriteLine("parameter Name " + param.Name.ToString());
            Console.WriteLine("parameter Value " + param.Value.ToString());
        }*/
        /*
    static StatusBox Convert(StatusInfo info)
    {
        return new StatusBox(info.Name.ToString(), false);
    }

    /*static Field Convert(FlagInfo info)
    {
        Parameter<bool> flag = new Parameter<bool>();
        flag.Caption = info.Name;
        flag.Value = false;
        return flag;
    }*/

    static InfoBox Convert(ParameterInfo info)
    {
        return new InfoBox(info.Name.ToString(), "-999", info.Units.ToString());
    }
    #endregion

    #region Доп. методы
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}