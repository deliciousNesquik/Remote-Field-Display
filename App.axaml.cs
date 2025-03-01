using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RFD.ViewModels;
using RFD.Views;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using NPFGEO.LWD.Net;
using RFD.Models;
using DateTime = System.DateTime;

namespace RFD;

public class App : Application
{
    public static App Instance => (App)Current!; // Получаем текущий экземпляр приложения
    
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

    public override void Initialize()
    {
        
        AvaloniaXamlLoader.Load(this);
        // Проверяем, не работает ли приложение в режиме дизайнера
        if (Design.IsDesignMode)
        {
            return;
        }
        try
        {
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
        catch (Exception e)
        {
            //TODO
            //Создать уведомление пользователю о ошибке
            //создания вещательного канала между программой RFD и LWD
            
            _client = null!;
            _listener = null!;
            
            //В случае возникшей ошибки выдается такое сообщение и сама ошибка
            Console.WriteLine($"[{DateTime.Now}] - [Error creating _client and _listener] - [{e}]");
        }
        
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            //Условие для desktop приложений, которые поддерживают оконную систему отображения приложений
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                _mainWindowViewModel = new MainWindowViewModel();
                MainWindow mainWindow = new() { DataContext = _mainWindowViewModel, };
                desktop.MainWindow = mainWindow;
                
                //Вызывается окно автоматического подключения для соединения
                _mainWindowViewModel.OpenAutomaticConnecting();
                
                break;
            }
        }
        base.OnFrameworkInitializationCompleted();
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
        //Установка Магнитное склонение и Смещение поверхности инструмента
        _mainWindowViewModel.ParametersSectionViewModel.MagneticDeclination = e.Settings.InfoParameters.MagneticDeclination;
        _mainWindowViewModel.ParametersSectionViewModel.ToolfaceOffset = e.Settings.InfoParameters.ToolfaceOffset;
        
        //Установка мишени
        _mainWindowViewModel.TargetSectionViewModel.SetSector(
            (e.Settings.Target.SectorDirection - e.Settings.Target.SectorWidth / 2), 
            (e.Settings.Target.SectorDirection + e.Settings.Target.SectorWidth / 2)
            );

        //e.Settings.Statuses = empty data
        Console.WriteLine("ReceiveSettingsEventArgs---------STATUS BLOCKS-----------");
        foreach (var flag in e.Settings.Statuses)
        {
            Console.WriteLine($"NAME[{flag.Name}] STATUS[{flag.Palette}]");
        }
        Console.WriteLine("ReceiveSettingsEventArgs-------------------------------");
        
        
        Console.WriteLine("ReceiveSettingsEventArgs---------PARAMETERS BLOCKS-----------");
        foreach (var parameter in e.Settings.Parameters)
        {
            Console.WriteLine($"NAME[{parameter.Name}] FLOAT[{parameter.Float}] UNITS[{parameter.Units}]");
        }
        Console.WriteLine("ReceiveSettingsEventArgs--------------------------------------");
        
        Console.WriteLine("ReceiveSettingsEventArgs---------PARAMETERS BLOCKS-----------");
        foreach (var param in e.Settings.DateTimeParameters)
        {
            Console.WriteLine($"NAME[{param.Name}] ALIAS[{param.Alias}]");
        }
        Console.WriteLine("ReceiveSettingsEventArgs--------------------------------------");
        
        //Очистка старых блоков
        //_mainWindowViewModel.InformationSectionViewModel.ClearInfoBox();
        
        //Установка информационных блоков
        foreach (var parameter in e.Settings.Parameters)
        {
            foreach (var infoBox in _mainWindowViewModel.InformationSectionViewModel.InfoBlockList)
            {
                if (infoBox.Title == parameter.Name)
                {
                    infoBox.Inscription = parameter.Units;
                }
            }
        }
    }
    
    /// <summary>
    /// Прием клиентом данных
    /// </summary>
    /// <param name="sender">Отправитель</param>
    /// <param name="e">Аргументы события</param>
   private void Client_ReceiveData(object? sender, ReceiveDataEventArgs e)
   {
       //Установка точек на мишени
       foreach (var i in e.Data.TargetPoints)
       {
           _mainWindowViewModel.TargetSectionViewModel.SetPoint((int)i.Order, i.Angle);
           _mainWindowViewModel.ParametersSectionViewModel.TimeStamp = i.TimeStamp;
           _mainWindowViewModel.ParametersSectionViewModel.Angle = i.Angle;
           _mainWindowViewModel.ParametersSectionViewModel.ToolfaceType = i.ToolfaceType.ToString();
       }
       
       //e.Data.Statuses = empty
       foreach (var flag in e.Data.Flags)
       {
           _mainWindowViewModel.StatusSectionViewModel.AddStatusBox(new StatusBox(flag.Name, flag.Value)); }
       
       _mainWindowViewModel.InformationSectionViewModel.ClearInfoBox();
       foreach (var parameter in e.Data.Parameters)
       {
           _mainWindowViewModel.InformationSectionViewModel.AddInfoBox(new InfoBox(parameter.Name, parameter.Value));
       }
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

}