using System.Globalization;
using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using NLog;
using NPFGEO.LWD.Net;
using RFD.Models;
using RFD.Services;
using RFD.ViewModels;
using RFD.Views;

namespace RFD;

public class App : Application
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>Переменная отвечающая за ресурс который сделал отмену</summary>
    private CancellationTokenSource _cancelTokenSource = new();


    private DataObject _dataObj = new();

    /// <summary>Объект класса слушателя который прослушивает сообщения от сервера и обрабатывает их</summary>
    private ServerListener _listener = null!;

    /// <summary>Создание объекта класса главного окна, для открытия его и управления внутренними методами и объектами</summary>
    private MainWindowViewModel _mainWindowViewModel = null!;

    /// <summary>Переменная отвечающая за автоматическое переподключение в случаях случайного отключения</summary>
    private bool _needAutoReconnect = true;

    /// <summary>Переменная отвечающая за отмену от выполнения некоторых работ</summary>
    private CancellationToken _token;

    /// <summary>Объект класса клиента который взаимодействует с подключением к серверу</summary>
    public Client Client = null!;


    public static App Instance => (App)Current!; // Получаем текущий экземпляр приложения

    /// <summary>Статус подключения к серверу</summary>
    public bool IsConnected => Client.Connected;

    /// <summary>Ip-адрес к подключенному серверу</summary>
    public string Address => Client.Connected ? Client.Address.ToString() : "Нет подключения";

    private static void ConfigureLogging()
    {
        try
        {
            LogManager.Setup().LoadConfigurationFromFile("NLog.config");
            Logger.Info("Логирование настроено.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки конфигурации NLog: {ex.Message}");
        }
    }

    public override void Initialize()
    {
        //Включаем логирование для отслеживания действий программы.
        ConfigureLogging();
        Logger.Info("Инициализация приложения...");
        try
        {
            AvaloniaXamlLoader.Load(this);
        }
        catch (Exception ex)
        {
            Logger.Error($"Ошибка инициализации приложения: {ex}.");
            return;
        }

        Logger.Info("Инициализация прошла успешно.");

        if (Design.IsDesignMode)
        {
            Logger.Warn("Приложение запущено в Design режиме.");
            return;
        }

        Logger.Info("Инициализация MainWindowViewModel...");
        try
        {
            _mainWindowViewModel = new MainWindowViewModel();
        }
        catch (Exception ex)
        {
            Logger.Error($"Ошибка инициализации MainWindowViewModel: {ex}.");
            return;
        }

        Logger.Info("Инициализация MainWindowViewModel прошла успешно.");

        Logger.Info("Инициализация Client...");
        try
        {
            Client = new Client();
        }
        catch (Exception ex)
        {
            Logger.Error($"Ошибка инициализации Client: {ex}.");
            return;
        }

        Logger.Info("Инициализация Client прошла успешно.");

        Logger.Info("Инициализация ServerListener...");
        try
        {
            _listener = new ServerListener();
        }
        catch (Exception ex)
        {
            Logger.Error($"Ошибка инициализации ServerListener: {ex}.");
            return;
        }

        Logger.Info("Инициализация ServerListener прошла успешно.");

        Logger.Info("Инициализация _token...");
        try
        {
            _token = _cancelTokenSource.Token;
        }
        catch (Exception ex)
        {
            Logger.Error($"Ошибка инициализации _token: {ex}.");
            return;
        }

        Logger.Info("Инициализация _token прошла успешно.");


        Logger.Info("Привязка метода Client_ReceiveData к Client.ReceiveData.");
        Client.ReceiveData += Client_ReceiveData;

        Logger.Info("Привязка метода Client_ReceiveSettings к Client.ReceiveSettings.");
        Client.ReceiveSettings += Client_ReceiveSettings;

        Logger.Info("Привязка метода Client_Disconnected к Client.Disconnected.");
        Client.Disconnected += Client_Disconnected;

        Logger.Info("Привязка метода Client_ConnectedStatusChanged к Client.ConnectedStatusChanged.");
        Client.ConnectedStatusChanged += Client_ConnectedStatusChanged;

        Logger.Info("Привязка метода Listener_ReceiveBroadcast к Client.ReceiveBroadcast.");
        _listener.ReceiveBroadcast += Listener_ReceiveBroadcast;

        Logger.Info("Старт ServerListener для прослушивания сервера.");
        _listener.Start();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                ThemeManager.ApplyTheme(AppTheme.Light); // или загрузи из настроек

                Logger.Info("Привязка метода закрытия приложения...");
                try
                {
                    desktop.Exit += OnExit;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка привязки метода закрытия приложения: {ex}");
                    return;
                }

                Logger.Info("Успешно привязался метод для закрытия приложения");

                Logger.Info("Успешная подписка на событие изменения темы приложения");

                Logger.Info("Инициализация главного окна...");
                MainWindow mainWindow = new() { DataContext = _mainWindowViewModel };

                Logger.Info("Присваивание (desktop.MainWindow = mainWindow)...");
                desktop.MainWindow = mainWindow;


                break;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Logger.Info("Закрытие приложения...");

        Logger.Info("Остановка прослушивания ServerListener...");
        try
        {
            _listener.Stop();
        }
        catch (Exception ex)
        {
            Logger.Error($"Ошибка остановки ServerListener: {ex}");
            return;
        }

        Logger.Info("Успешно остановлено прослушивание ServerListener.");

        _needAutoReconnect = false;

        Client.ReceiveData -= Client_ReceiveData;
        Client.ReceiveSettings -= Client_ReceiveSettings;
        Client.Disconnected -= Client_Disconnected;
        Client.ConnectedStatusChanged -= Client_ConnectedStatusChanged;
        Client.Dispose();
    }

    /// <summary>
    ///     Слушатель получает широковещательную передачу
    /// </summary>
    /// <param name="sender">Отправитель</param>
    /// <param name="e">Аргументы события</param>
    private void Listener_ReceiveBroadcast(object? sender, ReceiveBroadcastEventArgs e)
    {
        Logger.Info("Вызов Listener_ReceiveBroadcast...");
        //Проверка состояния соединения
        Logger.Info("Проверка состояния подключения к серверу.");
        if (Client.Connected)
        {
            Logger.Info("Приложение подключено к серверу.");
            return;
        }

        //Остановка прослушивания
        Logger.Info("Остановка прослушивания ServerListener.");
        _listener.Stop();

        //Переподключение
        Logger.Info($"Попытка соединения с сервером по адресу: {e.Server.Address}");
        Client.Address = e.Server.Address;

        if (Client.Connect())
            Logger.Info($"Успешное подключение к: {Client.Address}");
        else
            Logger.Error($"Ошибка подключения к: {Client.Address}");

        _mainWindowViewModel.OnConnectionStateChanged();
    }

    /// <summary>
    ///     Прием клиентом настроек
    /// </summary>
    /// <param name="sender">Отправитель</param>
    /// <param name="e">Аргументы события</param>
    private void Client_ReceiveSettings(object? sender, ReceiveSettingsEventArgs e)
    {
        Logger.Info("Получение настроек от сервера...");
        try
        {
            Dispatcher.UIThread.InvokeAsync((Action)Action, DispatcherPriority.Background);
        }
        catch (Exception ex)
        {
            Logger.Error($"Ошибка запуска потока SetSettings - {ex}");
        }


        void Action()
        {
            SetSettings(e.Settings);
        }
    }

    /// <summary>
    ///     Прием клиентом данных
    /// </summary>
    /// <param name="sender">Отправитель</param>
    /// <param name="e">Аргументы события</param>
    private void Client_ReceiveData(object? sender, ReceiveDataEventArgs e)
    {
        Logger.Info("Получение данных от сервера...");
        try
        {
            Dispatcher.UIThread.InvokeAsync(() => { SetData(e.Data); });
        }
        catch (Exception ex)
        {
            Logger.Error($"Ошибка запуска потока SetData - {ex}");
        }
    }

    /// <summary>
    ///     Клиент отключился
    /// </summary>
    /// <param name="sender">Отправитель</param>
    /// <param name="e">Аргументы события</param>
    private void Client_Disconnected(object? sender, EventArgs e)
    {
        Logger.Warn($"Отключение от сервера: {Client.Address}");
        _mainWindowViewModel.OnConnectionStateChanged();

        //Проверка на автоматическое переподключение
        if (!_needAutoReconnect) return;

        _cancelTokenSource = new CancellationTokenSource();
        _token = _cancelTokenSource.Token;
        var action = () =>
        {
            var address = Client.Address;
            Logger.Info("Попытка подключения к серверу пока клиент не подключится...");
            while (!_token.IsCancellationRequested && !Client.Connected)
            {
                Reconnect(address);
                _mainWindowViewModel.OnConnectionStateChanged();
            }
        };
        Task.Factory.StartNew(action, _token);
    }

    /// <summary>
    ///     Смена статуса подключения у клиента
    /// </summary>
    /// <param name="sender">Отправитель</param>
    /// <param name="e">Аргументы события</param>
    private void Client_ConnectedStatusChanged(object? sender, EventArgs e)
    {
        Logger.Info("Вызов Client_ConnectedStatusChanged изменение статуса подключения.");
        _mainWindowViewModel.OnConnectionStateChanged();
    }

    /// <summary>
    ///     Выполняет автоматическое переподключение.
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
            if (Client.Connected)
                Client.Disconnect();

            // Запускаем слушатель снова
            _listener.Start();

            return true; // Успешное выполнение
        }
        catch (Exception ex)
        {
            Logger.Error($"Ошибка при выполнении авто-подключения: {ex.Message}.");
            return false; // Ошибка при выполнении
        }
    }

    /// <summary>
    ///     Инициирует процесс автоматического переподключения.
    /// </summary>
    /// <returns>Значение <c>true</c>, если подключение успешно, иначе <c>false</c>.</returns>
    public async Task<bool> AutoConnect()
    {
        _needAutoReconnect = false; // Отключаем авто-переподключение перед началом процесса

        var result = await AutoConnectAsync(); // Запускаем процесс переподключения

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
            Task.Delay(1000);

            if (Client.Connected)
                Client.Disconnect();

            Client.Address = IPAddress.Parse(address);
            _needAutoReconnect = true;
            if (Client.Connect())
            {
                Logger.Info($"Подключение к серверу: {Client.Address}.");
                return true;
            }

            Logger.Error($"Ошибка подключения к серверу: {Client.Address}.");
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error($"Ошибка подключения к серверу: {ex}.");
            return false;
        }
    }

    private void Reconnect(IPAddress address)
    {
        _needAutoReconnect = false;
        try
        {
            _listener.Stop();

            if (Client.Connected)
                Client.Disconnect();

            Client.Address = address;
            if (Client.Connect())
                Logger.Info($"Подключение к серверу: {Client.Address}.");
            else
                Logger.Error($"Ошибка подключения к серверу: {Client.Address}.");
        }
        catch (Exception ex)
        {
            Logger.Error($"Ошибка подключения к серверу: {ex}.");
        }

        _needAutoReconnect = true;
    }

    public void Disconnect()
    {
        _needAutoReconnect = false;
        _listener.Stop();

        if (Client.Connected != true) return;
        Logger.Info($"Отключение от сервера: {Client.Address}");
        Client.Disconnect();
    }


    private void SetSettings(Settings settings)
    {
        Logger.Info("[SetSettings] Начало установки настроек");

        _mainWindowViewModel.InformationSectionViewModel.ClearInfoBox();
        _mainWindowViewModel.StatusSectionViewModel.ClearStatusBox();

        Logger.Info("[SetSettings] Очистка информационных и статусных блоков завершена");

        foreach (var flag in settings.Flags)
        {
            Logger.Info($"[SetSettings] Добавление флага: {flag.Name}");
            _mainWindowViewModel.StatusSectionViewModel.AddStatusBox(new StatusBox(flag.Name));
        }

        foreach (var flag in settings.Statuses)
        {
            Logger.Info($"[SetSettings] Добавление статуса: {flag.Name}");
            _mainWindowViewModel.StatusSectionViewModel.AddStatusBox(new StatusBox(flag.Name));
        }

        foreach (var param in settings.Parameters)
        {
            Logger.Info($"[SetSettings] Добавление параметра: {param.Name} ({param.Units})");
            _mainWindowViewModel.InformationSectionViewModel.AddInfoBox(new InfoBox(param.Name, "-", param.Units));
        }

        Logger.Info("[SetSettings] Установка параметров интерфейса");

        _mainWindowViewModel.TargetSectionViewModel.MagneticDeclination = settings.InfoParameters.MagneticDeclination;
        _mainWindowViewModel.TargetSectionViewModel.ToolfaceOffset = settings.InfoParameters.ToolfaceOffset;

        Logger.Info("[SetSettings] Установка параметров TargetSection");

        _mainWindowViewModel.TargetSectionViewModel.FromCenterToBorder = settings.Target.FromCenterToBorder;
        _mainWindowViewModel.TargetSectionViewModel.Capacity = settings.Target.Capacity;
        _mainWindowViewModel.TargetSectionViewModel.IsHalfMode = settings.Target.IsHalfMode;
        _mainWindowViewModel.TargetSectionViewModel.GridFrequency = settings.Target.GridFrequency;
        _mainWindowViewModel.TargetSectionViewModel.FontSize = settings.Target.FontSize;
        (_mainWindowViewModel.TargetSectionViewModel.RingWidth,
                _mainWindowViewModel.TargetSectionViewModel.RingThickness) =
            (settings.Target.RingWidth, settings.Target.RingWidth);
        _mainWindowViewModel.TargetSectionViewModel.DefaultRadius = settings.Target.DefaultRadius;
        _mainWindowViewModel.TargetSectionViewModel.ReductionFactor = settings.Target.ReductionFactor;

        Logger.Info("[SetSettings] Установка темы");
        ThemeManager.ApplyTheme(settings.ThemeStyle == "LightTheme" ? AppTheme.Light : AppTheme.Dark);

        Logger.Info("[SetSettings] Установка сектора");
        _mainWindowViewModel.TargetSectionViewModel.SetSector(
            settings.Target.SectorDirection - settings.Target.SectorWidth / 2,
            settings.Target.SectorDirection + settings.Target.SectorWidth / 2
        );

        Logger.Info("[SetSettings] Вызов SetData");
        SetData(_dataObj);
    }

    private void SetData(DataObject data)
    {
        Logger.Info("[SetData] Начало обработки данных");
        _dataObj = DataObject.Union(_dataObj, data);

        var targetPoints = data.TargetPoints.ToList();
        for (var i = 0; i < targetPoints.Count; i++)
        {
            Logger.Info(
                $"[SetData] Обработка точки {i}: Угол {targetPoints[i].Angle}, Toolface {targetPoints[i].ToolfaceType}");
            _mainWindowViewModel.TargetSectionViewModel.Angle = Math.Round(targetPoints[i].Angle, 2);
            _mainWindowViewModel.TargetSectionViewModel.ToolfaceType =
                targetPoints[i].ToolfaceType.ToString().Substring(0, 1);

            if (_mainWindowViewModel.TargetSectionViewModel.FromCenterToBorder)
            {
                Logger.Info($"[SetData] Установка точки (FromCenterToBorder): {targetPoints[i].Value}");
                try
                {
                    _mainWindowViewModel.TargetSectionViewModel.SetPoint(
                        _mainWindowViewModel.TargetSectionViewModel.Capacity - i - 1,
                        targetPoints[i].Value);
                }
                catch (Exception e)
                {
                    Logger.Error($"Ошибка при создании точки (FromCenterToBorder) {e.Message}");
                }
            }
            else
            {
                Logger.Info($"[SetData] Установка точки (Not FromCenterToBorder): {targetPoints[i].Value}");
                try
                {
                    _mainWindowViewModel.TargetSectionViewModel.SetPoint(
                        _mainWindowViewModel.TargetSectionViewModel.Capacity - targetPoints.Count + i,
                        targetPoints[i].Value);
                }
                catch (Exception e)
                {
                    Logger.Error($"Ошибка при создании точки (Not FromCenterToBorder) {e.Message}");
                }
            }
        }

        Logger.Info("[SetData] Обработка флагов и статусов");

        foreach (var t in data.Flags)
        foreach (var t2 in _mainWindowViewModel.StatusSectionViewModel.InfoStatusList)
            if (t2.Header == t.Name)
            {
                Logger.Info($"[SetData] Флаг {t.Name} -> {t.Value}");
                t2.Status = t.Value;
            }

        foreach (var t in data.Statuses)
        foreach (var t2 in _mainWindowViewModel.StatusSectionViewModel.InfoStatusList)
            if (t2.Header == t.Name)
            {
                Logger.Info($"[SetData] Статус {t.Name} -> {t.Value}");
                t2.Status = Convert.ToBoolean(t.Value);
            }

        Logger.Info("[SetData] Обработка параметров");

        foreach (var t in data.Parameters)
        foreach (var t2 in _mainWindowViewModel.InformationSectionViewModel.InfoBlockList)
            if (t2.Title == t.Name)
            {
                Logger.Info($"[SetData] Параметр {t.Name} -> {t.Value}");
                t2.Content = double.Round(t.Value, 2).ToString(CultureInfo.CurrentCulture);
            }

        Logger.Info("[SetData] Установка времени");
        _mainWindowViewModel.TargetSectionViewModel.SetTime(data);
    }
}