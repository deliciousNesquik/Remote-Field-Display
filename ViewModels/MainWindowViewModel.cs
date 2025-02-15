using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using RFD.Models;
using RFD.UserControls;

namespace RFD.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    #region Переменные: Геофизические параметры
    public TargetSectionViewModel TargetSectionViewModel { get; set; }
    public ParametersSectionViewModel ParametersSectionViewModel { get; set; }
    public InformationSectionViewModel InformationSectionViewModel { get; set; }
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
        
    private bool _isModalWindowOpen;
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
    
    private bool _isFirstCellVisible;
    
    private bool IsFirstCellVisible
    {
        get => _isFirstCellVisible;
        set
        {
            _isFirstCellVisible = value;
            OnPropertyChanged();
        }
    }
    private bool _isSecondCellVisible;
    public bool IsSecondCellVisible
    {
        get => _isSecondCellVisible;
        set
        {
            _isSecondCellVisible = value;
            OnPropertyChanged();
        }
    }
    private bool _isThirdCellVisible;
    public bool IsThirdCellVisible
    {
        get => _isThirdCellVisible;
        set
        {
            _isThirdCellVisible = value;
            OnPropertyChanged();
        }
    }
    private bool _isFourCellVisible;
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
    private static App? Model => Application.Current as App;
    #endregion
        
    public MainWindowViewModel()
    {
        TargetSectionViewModel = new TargetSectionViewModel();
        ParametersSectionViewModel = new ParametersSectionViewModel();
        InformationSectionViewModel = new InformationSectionViewModel();
        
        FirstCell = new TargetSection
        {
            DataContext = TargetSectionViewModel
        };
        SecondCell = new ParametersSection
        {
            DataContext = ParametersSectionViewModel
        };
        ThirdCell = new InformationSection
        {
            DataContext = InformationSectionViewModel
        };
        FourCell = new InformationSection
        {
            DataContext = InformationSectionViewModel
        };

        //Использовать только эти два методы для создания сектора и его очистки
        TargetSectionViewModel.SetSector(45, 135);
        TargetSectionViewModel.ClearSector();
        
        
        InformationSectionViewModel.AddInfoBox(new InfoBox("Высота блока", "-", "м"));
        InformationSectionViewModel.AddInfoBox(new InfoBox("Высота блока", "-", "м"));
        InformationSectionViewModel.ClearInfoBox();
        
        
        ParametersSectionViewModel.MagneticDeclination = 10.0;
        ParametersSectionViewModel.ToolfaceOffset = 12.0;
        ParametersSectionViewModel.MagneticDeclination = 0;
        ParametersSectionViewModel.ToolfaceOffset = 0;
        
        
        
        //Геофизические параметры заполнены для примера
        
        //InfoStatusList = [
        //    new ("Клинья", false),
        //    new ("Насос", false),
        //    new ("Забой", false),
        //];

        IsFirstCellVisible = true;
        IsSecondCellVisible = true;
        IsThirdCellVisible = true;
        IsFourCellVisible = true;
    
        App.ConnectionUpdated += UpdateConnecting;
        //App.SettingsUpdated += SetSettings;
            
        //Команды основного меню
        OpenAutomaticConnectingCommand = new RelayCommand(OpenAutomaticConnecting, () => !IsModalWindowOpen);
        OpenManualConnectingCommand = new RelayCommand(OpenManualConnecting, () => !IsModalWindowOpen);
        DisconnectCommand = new RelayCommand(Disconnect, () => ConnectionStatus);
        SettingsCommand = new RelayCommand((() => Console.WriteLine("Open settings")), () => true);
        TargetVisibleCommand = new RelayCommand(() =>
        {
            IsFirstCellVisible = !IsFirstCellVisible;
            Console.WriteLine(IsFirstCellVisible);
        });
        InformationVisibleCommand = new RelayCommand(() =>
        {
            IsThirdCellVisible = !IsThirdCellVisible;
            Console.WriteLine(IsThirdCellVisible);
        });
        ParametersVisibleCommand = new RelayCommand(() =>
        {
            IsSecondCellVisible = !IsSecondCellVisible;
            Console.WriteLine(IsSecondCellVisible);
        });
        StatusesVisibleCommand = new RelayCommand(() =>
        {
            IsFourCellVisible = !IsFourCellVisible;
            Console.WriteLine(IsFourCellVisible);
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
            Console.WriteLine("Попытка подключения к адресу: " + ip);
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
        }, cancellationTokenSource.Token);
    }
    #endregion

    #region Методы: Методы для соединения с сервером

    private void Disconnect()
    {
        /*if (Model != null)
        {
            Model.Disconnect();
        }*/
    }

    private void UpdateConnecting()
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

    /*static InfoBox Convert(ParameterInfo info)
    {
        return new InfoBox(info.Name.ToString(), "-999", info.Units.ToString());
    }*/
    #endregion

    #region Доп. методы
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}