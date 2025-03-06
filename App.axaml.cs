using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RFD.ViewModels;
using RFD.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Threading;
using ExCSS;
using NPFGEO.LWD.Net;
using RFD.Models;
using DateTime = System.DateTime;

namespace RFD;

public class App : Application
{
    public static App Instance => (App)Current!; // Получаем текущий экземпляр приложения
    private static readonly Uri LightThemeUri = new("avares://RFD/Themes/Light.axaml");
    private static readonly Uri DarkThemeUri = new("avares://RFD/Themes/Dark.axaml");

    
    
    /// <summary>Создание объекта класса главного окна, для открытия его и управления внутренними методами и объектами</summary>
    private MainWindowViewModel _mainWindowViewModel = null!;
    
    /// <summary>Объект класса клиента который взаимодействует с подключением к серверу</summary>
    private Client _client = null!;
    /// <summary>Объект класса слушателя который прослушивает сообщения от сервера и обрабатывает их</summary>
    private ServerListener _listener = null!;
    /// <summary>Переменная отвечающая за автоматическое переподключение в случаях случайного отключения</summary>
    private bool _needAutoReconnect = true;
    
    /// <summary>Статус подключения к серверу</summary>
    public bool IsConnected => _client.Connected;
    /// <summary>Ip-адрес к подключенному серверу</summary>
    public string Address => _client.Address == null ? "Не найдено" : _client.Address.ToString();

    /// <summary>Переменная отвечающая за ресурс который сделал отмену</summary>
    private CancellationTokenSource _cancelTokenSource = new();
    /// <summary>Переменная отвечающая за отмену от выполнения некоторых работ</summary>
    private CancellationToken _token;
    // Событие для подписчиков
    
    
    NPFGEO.LWD.Net.DataObject _dataObj = new NPFGEO.LWD.Net.DataObject();
    public event Action<string>? ThemeChanged;

    public override void Initialize()
    {

        AvaloniaXamlLoader.Load(this);
        // Проверяем, не работает ли приложение в режиме дизайнера
        if (Design.IsDesignMode)
        {
            return;
        }
        
        //Инициализация клиента и слушателя для дальнейшего взаимодействия с сервером
        _client = new();
        _client.ReceiveData += Client_ReceiveData;
        _client.ReceiveSettings += Client_ReceiveSettings;
        _client.Disconnected += Client_Disconnected;
        _client.ConnectedStatusChanged += Client_ConnectedStatusChanged;

        _listener = new();
        _listener.ReceiveBroadcast += Listener_ReceiveBroadcast;
        _listener.Start();

    }

    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            //Условие для desktop приложений, которые поддерживают оконную систему отображения приложений
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                desktop.Exit += OnExit;
                this.GetObservable(ActualThemeVariantProperty).Subscribe(OnThemeChanged);
                
                _mainWindowViewModel = new MainWindowViewModel();
                MainWindow mainWindow = new() { DataContext = _mainWindowViewModel, };
                desktop.MainWindow = mainWindow;
                
                break;
            }
        }
        base.OnFrameworkInitializationCompleted();
    }

    protected void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _needAutoReconnect = false;
        _listener.Stop();
        if (_client != null)
        {
            _client.ReceiveData -= Client_ReceiveData;
            _client.ReceiveSettings -= Client_ReceiveSettings;
            _client.Disconnected -= Client_Disconnected;
            _client.ConnectedStatusChanged -= Client_ConnectedStatusChanged;
            _client.Dispose();
        }
    }
    
    private void OnThemeChanged(ThemeVariant newTheme)
    {
        ThemeChanged?.Invoke(newTheme.Key.ToString());
    }

    public void ClearBufferedData()
    {
        _dataObj = new NPFGEO.LWD.Net.DataObject();
    }
    
    /// <summary>
    /// Слушатель получает широковещательную передачу
    /// </summary>
    /// <param name="sender">Отправитель</param>
    /// <param name="e">Аргументы события</param>
    void Listener_ReceiveBroadcast(object? sender, ReceiveBroadcastEventArgs e)
    {
        //Проверка состояния соединения
        if (_client.Connected) return;

        //Остановка прослушивания
        _listener.Stop();

        //Переподключение
        _client.Address = e.Server.Address;
        _client.Connect();
        
        //Обновление интерфейса для отображения подключения
        _mainWindowViewModel.UpdateConnecting(this);
        Console.WriteLine($"[{DateTime.Now}] - [Successful connection] - [ip address: {_client.Address}]");
    }

    /// <summary>
    /// Прием клиентом настроек
    /// </summary>
    /// <param name="sender">Отправитель</param>
    /// <param name="e">Аргументы события</param>
    private void Client_ReceiveSettings(object? sender, ReceiveSettingsEventArgs e)
    {
        Action action = () =>
        {
            SetSettings(e.Settings);
        };

        Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Background);
        
    }
    
    /// <summary>
    /// Прием клиентом данных
    /// </summary>
    /// <param name="sender">Отправитель</param>
    /// <param name="e">Аргументы события</param>
   private void Client_ReceiveData(object? sender, ReceiveDataEventArgs e)
   {
       Dispatcher.UIThread.InvokeAsync(() =>
       {
           SetData(e.Data);
       });
   }

    /// <summary>
    /// Клиент отключился
    /// </summary>
    /// <param name="sender">Отправитель</param>
    /// <param name="e">Аргументы события</param>
    private void Client_Disconnected(object? sender, EventArgs e)
    {
        Console.WriteLine($"[{DateTime.Now}] - [Disconnecting from the server] - [{_client.Address}]");
        _mainWindowViewModel.UpdateConnecting(this);

        //Проверка на автоматическое переподключение
        if (!_needAutoReconnect) return;
        
        _cancelTokenSource = new CancellationTokenSource();
        _token = _cancelTokenSource.Token;
        Action action = () =>
        {
            var address = _client.Address;
            while (!_token.IsCancellationRequested && !_client.Connected)
            {
                Reconnect(address);
                _mainWindowViewModel.UpdateConnecting(this);
            }
        };
        Task.Factory.StartNew(action, _token);
    }

    /// <summary>
    /// Смена статуса подключения у клиента
    /// </summary>
    /// <param name="sender">Отправитель</param>
    /// <param name="e">Аргументы события</param>
    private void Client_ConnectedStatusChanged(object? sender, EventArgs e)
    {
        _mainWindowViewModel.UpdateConnecting(this);
    }

    /// <summary>
    /// Выполняет автоматическое переподключение.
    /// </summary>
    /// <returns>Значение <c>true</c>, если подключение успешно, иначе <c>false</c>.</returns>
    private async Task<bool> AutoConnectAsync()
    {
        try
        {
            // Останавливаем слушатель входящих соединений (если он существует)
            _listener.Stop();

            // Отменяем текущий токен (если он существует)
            await _cancelTokenSource.CancelAsync();

            // Задержка перед повторным подключением
            await Task.Delay(2500, _token);

            // Если клиент всё ещё подключен, разрываем соединение
            if (_client.Connected)
                _client.Disconnect();

            // Запускаем слушатель снова
            _listener.Start();

            return true; // Успешное выполнение
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now}] - [Error during auto-activation] [{ex.Message}]");
            return false; // Ошибка при выполнении
        }
    }

    /// <summary>
    /// Инициирует процесс автоматического переподключения.
    /// </summary>
    /// <returns>Значение <c>true</c>, если подключение успешно, иначе <c>false</c>.</returns>
    public async Task<bool> AutoConnect()
    {
        _needAutoReconnect = false; // Отключаем авто-переподключение перед началом процесса

        bool result = await AutoConnectAsync(); // Запускаем процесс переподключения

        _needAutoReconnect = true; // После завершения снова включаем авто-переподключение

        return result; // Возвращаем результат переподключения
    }



    public bool Connect(string address)
    {
        _needAutoReconnect = false; 
        try 
        {
            _listener.Stop();

            _cancelTokenSource.Cancel();
            Thread.Sleep(1000);

            if (_client.Connected)
                _client.Disconnect();

            _client.Address = IPAddress.Parse(address);
            _client.Connect();
            Console.WriteLine($"[{DateTime.Now}] - [Connecting to the server] - [{_client.Address}]");
            _needAutoReconnect = true;
            return true; 
        }
        catch (Exception exc)
        {
            Console.WriteLine($"[{DateTime.Now}] - [Connection to the server failed] - [{exc}]");
            return false;
        }
        
    }

    private void Reconnect(IPAddress address)
    {
        _needAutoReconnect = false;
        try
        {
            _listener.Stop();

            if (_client.Connected)
                _client.Disconnect();

            _client.Address = address;
            _client.Connect();
            Console.WriteLine($"[{DateTime.Now}] - [Connecting to the server] - [{_client.Address}]");
        }
        catch (Exception exc)
        {
            Console.WriteLine($"[{DateTime.Now}] - [Connection to the server failed] - [{exc}]");
        }

        _needAutoReconnect = true;
    }

    public void Disconnect()
    {
        _needAutoReconnect = false;
        _listener.Stop();

        if (_client.Connected != true) return;
        Console.WriteLine($"[{DateTime.Now}] - [Disconnecting from the server] - [{_client.Connected}]");
        _client.Disconnect();
    }


    private void SetSettings(NPFGEO.LWD.Net.Settings settings)
    {
        _mainWindowViewModel.InformationSectionViewModel.ClearInfoBox();
        _mainWindowViewModel.StatusSectionViewModel.ClearStatusBox();
        
        foreach (var flag in settings.Flags)
        { _mainWindowViewModel.StatusSectionViewModel.AddStatusBox(new StatusBox(flag.Name, false)); }

        foreach (var flag in settings.Statuses)
        { _mainWindowViewModel.StatusSectionViewModel.AddStatusBox(new StatusBox(flag.Name, false)); }

        foreach (var param in settings.Parameters)
        { _mainWindowViewModel.InformationSectionViewModel.AddInfoBox(new InfoBox(param.Name, "-", param.Units)); }
        
        if (settings.InfoParameters != null)
        { 
            _mainWindowViewModel.ParametersSectionViewModel.MagneticDeclination = settings.InfoParameters.MagneticDeclination;
            _mainWindowViewModel.ParametersSectionViewModel.ToolfaceOffset = settings.InfoParameters.ToolfaceOffset;
        }

        _mainWindowViewModel.TargetSectionViewModel.FromCenterToBorder = settings.Target.FromCenterToBorder;
        _mainWindowViewModel.TargetSectionViewModel.Capacity = settings.Target.Capacity;
        _mainWindowViewModel.TargetSectionViewModel.IsHalfMode = settings.Target.IsHalfMode;
        _mainWindowViewModel.TargetSectionViewModel.GridFrequency = settings.Target.GridFrequency;
        _mainWindowViewModel.TargetSectionViewModel.FontSize = settings.Target.FontSize;
        (_mainWindowViewModel.TargetSectionViewModel.RingWidth, _mainWindowViewModel.TargetSectionViewModel.RingThickness) = (settings.Target.RingWidth, settings.Target.RingWidth);
        _mainWindowViewModel.TargetSectionViewModel.DefaultRadius = settings.Target.DefaultRadius;
        _mainWindowViewModel.TargetSectionViewModel.ReductionFactor = settings.Target.ReductionFactor;
        
        if (settings.Target != null) {
            _mainWindowViewModel.TargetSectionViewModel.SetSector(
                startAngle: settings.Target.SectorDirection - (settings.Target.SectorWidth / 2), 
                endAngle: settings.Target.SectorDirection + (settings.Target.SectorWidth / 2)
                );
        }
        
        SetData(_dataObj);
    }

    private void SetData(NPFGEO.LWD.Net.DataObject data)
    {
        _dataObj = NPFGEO.LWD.Net.DataObject.Union(_dataObj, data);

        if (data.TargetPoints != null)
        {
            IList<TargetPoint> targetPoints = data.TargetPoints.ToList();
            for (int i = 0; i < targetPoints.Count; i++)
            {
                _mainWindowViewModel.ParametersSectionViewModel.Angle = targetPoints[i].Angle;
                _mainWindowViewModel.ParametersSectionViewModel.ToolfaceType = targetPoints[i].ToolfaceType.ToString();
                
                if (_mainWindowViewModel.TargetSectionViewModel.FromCenterToBorder)
                {
                    _mainWindowViewModel.TargetSectionViewModel.SetPoint(
                        index: _mainWindowViewModel.TargetSectionViewModel.Capacity - i - 1,
                        angle: targetPoints[i].Value);
                }
                else
                {
                    _mainWindowViewModel.TargetSectionViewModel.SetPoint(
                        index: _mainWindowViewModel.TargetSectionViewModel.Capacity - targetPoints.Count + i,
                        angle: targetPoints[i].Value);
                }
            }
            
        }

        foreach (var t in data.Flags)
        { foreach (var t2 in _mainWindowViewModel.StatusSectionViewModel.InfoStatusList)
            { if (t2.Header == t.Name)
                { t2.Status = t.Value;
                }
            }
        }

        foreach (var t in data.Statuses)
        { foreach (var t2 in _mainWindowViewModel.StatusSectionViewModel.InfoStatusList)
            { if (t2.Header == t.Name)
                { t2.Status = Convert.ToBoolean(t.Value);
                }
            }
        }

        foreach (var t in data.Parameters)
        { foreach (var t2 in _mainWindowViewModel.InformationSectionViewModel.InfoBlockList)
            { if (t2.Title == t.Name)
                { t2.Content = double.Round(t.Value, 2).ToString(CultureInfo.CurrentCulture); }
            }
        }
           
        _mainWindowViewModel.ParametersSectionViewModel.SetTime(data);
    }
    
}