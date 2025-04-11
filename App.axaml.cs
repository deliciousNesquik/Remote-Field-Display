using System.Globalization;
using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using RFD.Models;
using RFD.Services;
using RFD.ViewModels;
using RFD.Views;

using NPFGEO.LWD.Net;
using NLog;
using RFD.Interfaces;

namespace RFD;

public class App : Application
{
    private readonly ILoggerService _logger;
    private readonly IConnectionService _connectionService;
    private readonly IBroadcastListener _broadcastListener;
    private MainWindowViewModel _mainWindowViewModel;
    private DataObject _dataObj = new();

    public App()
    {
        _logger = Program.Services!.GetRequiredService<ILoggerService>();
        _connectionService = Program.Services!.GetRequiredService<IConnectionService>();
        _broadcastListener = Program.Services!.GetRequiredService<IBroadcastListener>();
    }

    public override void Initialize()
    {
        _logger.Info("Инициализация приложения...");
        
        try
        {
            AvaloniaXamlLoader.Load(this);
            
            if (Design.IsDesignMode)
            {
                _logger.Warn("Приложение запущено в Design режиме.");
                return;
            }
            SubscribeToEvents();
            _broadcastListener.Start();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка инициализации приложения");
            throw;
        }
    }
    
    private void SubscribeToEvents()
    {
        _connectionService.ReceiveData += Client_ReceiveData;
        _connectionService.ReceiveSettings += Client_ReceiveSettings;
        _connectionService.Disconnected += Client_Disconnected;
        _connectionService.ConnectedStatusChanged += Client_ConnectedStatusChanged;
        _broadcastListener.ReceiveBroadcast += Listener_ReceiveBroadcast;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ThemeManager.ApplyTheme(AppTheme.Light);
            desktop.Exit += OnExit;

            
            
            // Получаем MainWindowViewModel из DI контейнера
            _mainWindowViewModel = Program.Services!.GetRequiredService<MainWindowViewModel>();
            
            // Создаем главное окно
            desktop.MainWindow = new MainWindow
            {
                DataContext = _mainWindowViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _logger.Info("Закрытие приложения...");
        UnsubscribeFromEvents();
        _broadcastListener.Dispose();
        _connectionService.Dispose();
    }
    
    private void UnsubscribeFromEvents()
    {
        _connectionService.ReceiveData -= Client_ReceiveData;
        _connectionService.ReceiveSettings -= Client_ReceiveSettings;
        _connectionService.Disconnected -= Client_Disconnected;
        _connectionService.ConnectedStatusChanged -= Client_ConnectedStatusChanged;
        _broadcastListener.ReceiveBroadcast -= Listener_ReceiveBroadcast;
    }

    
    private void Listener_ReceiveBroadcast(object? sender, ReceiveBroadcastEventArgs e)
    {
        _logger.Info("Вызов Listener_ReceiveBroadcast...");
        //Проверка состояния соединения
        _logger.Info("Проверка состояния подключения к серверу.");
        if (_connectionService.Connected)
        {
            _logger.Info("Приложение подключено к серверу.");
            return;
        }

        //Остановка прослушивания
        _logger.Info("Остановка прослушивания ServerListener.");
        _broadcastListener.Stop();

        //Переподключение
        _logger.Info($"Попытка соединения с сервером по адресу: {e.Server.Address}");
        
        if (_connectionService.ConnectAsync(e.Server.Address.ToString()).Result)
            _logger.Info($"Успешное подключение к: {_connectionService.Address}");
        else
            _logger.Error($"Ошибка подключения к: {_connectionService.Address}");

        _mainWindowViewModel.OnConnectionStateChanged(_connectionService.Address, _connectionService.Connected);
    }
    private void Client_ReceiveSettings(object? sender, ReceiveSettingsEventArgs e)
    {
        _logger.Info("Получение настроек от сервера...");
        try
        {
            Dispatcher.UIThread.InvokeAsync((Action)Action, DispatcherPriority.Background);
        }
        catch (Exception ex)
        {
            _logger.Error($"Ошибка запуска потока SetSettings - {ex}");
        }


        void Action()
        {
            SetSettings(e.Settings);
        }
    }
    private void Client_ReceiveData(object? sender, ReceiveDataEventArgs e)
    {
        _logger.Info("Получение данных от сервера...");
        try
        {
            Dispatcher.UIThread.InvokeAsync(() => { SetData(e.Data); });
        }
        catch (Exception ex)
        {
            _logger.Error($"Ошибка запуска потока SetData - {ex}");
        }
    }
    private void Client_Disconnected(object? sender, EventArgs e)
    {
        _logger.Warn($"Отключение от сервера: {_connectionService.Address}");
        _mainWindowViewModel.OnConnectionStateChanged(_connectionService.Address, _connectionService.Connected);
    }
    private void Client_ConnectedStatusChanged(object? sender, EventArgs e)
    {
        _logger.Info("Вызов Client_ConnectedStatusChanged изменение статуса подключения.");
        _mainWindowViewModel.OnConnectionStateChanged(_connectionService.Address, _connectionService.Connected);
    }

    
    private void SetSettings(Settings settings)
    {
        _logger.Info("[SetSettings] Начало установки настроек");

        _mainWindowViewModel.InformationSectionViewModel.ClearInfoBox();
        _mainWindowViewModel.StatusSectionViewModel.ClearStatusBox();

        _logger.Info("[SetSettings] Очистка информационных и статусных блоков завершена");

        foreach (var flag in settings.Flags)
        {
            _logger.Info($"[SetSettings] Добавление флага: {flag.Name}");
            _mainWindowViewModel.StatusSectionViewModel.AddStatusBox(new StatusBox(flag.Name));
        }

        foreach (var flag in settings.Statuses)
        {
            _logger.Info($"[SetSettings] Добавление статуса: {flag.Name}");
            _mainWindowViewModel.StatusSectionViewModel.AddStatusBox(new StatusBox(flag.Name));
        }

        foreach (var param in settings.Parameters)
        {
            _logger.Info($"[SetSettings] Добавление параметра: {param.Name} ({param.Units})");
            _mainWindowViewModel.InformationSectionViewModel.AddInfoBox(new InfoBox(param.Name, "-", param.Units));
        }

        _logger.Info("[SetSettings] Установка параметров интерфейса");

        _mainWindowViewModel.TargetSectionViewModel.MagneticDeclination = settings.InfoParameters.MagneticDeclination;
        _mainWindowViewModel.TargetSectionViewModel.ToolfaceOffset = settings.InfoParameters.ToolfaceOffset;

        _logger.Info("[SetSettings] Установка параметров TargetSection");

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

        _logger.Info("[SetSettings] Установка темы");
        ThemeManager.ApplyTheme(settings.ThemeStyle == "LightTheme" ? AppTheme.Light : AppTheme.Dark);

        _logger.Info("[SetSettings] Установка сектора");
        _mainWindowViewModel.TargetSectionViewModel.SetSector(
            settings.Target.SectorDirection - settings.Target.SectorWidth / 2,
            settings.Target.SectorDirection + settings.Target.SectorWidth / 2
        );

        _logger.Info("[SetSettings] Вызов SetData");
        SetData(_dataObj);
    }

    private void SetData(DataObject data)
    {
        _logger.Info("[SetData] Начало обработки данных");
        _dataObj = DataObject.Union(_dataObj, data);

        var targetPoints = data.TargetPoints.ToList();
        for (var i = 0; i < targetPoints.Count; i++)
        {
            _logger.Info(
                $"[SetData] Обработка точки {i}: Угол {targetPoints[i].Angle}, Toolface {targetPoints[i].ToolfaceType}");
            _mainWindowViewModel.TargetSectionViewModel.Angle = Math.Round(targetPoints[i].Angle, 2);
            _mainWindowViewModel.TargetSectionViewModel.ToolfaceType =
                targetPoints[i].ToolfaceType.ToString().Substring(0, 1);

            if (_mainWindowViewModel.TargetSectionViewModel.FromCenterToBorder)
            {
                _logger.Info($"[SetData] Установка точки (FromCenterToBorder): {targetPoints[i].Value}");
                try
                {
                    _mainWindowViewModel.TargetSectionViewModel.SetPoint(
                        _mainWindowViewModel.TargetSectionViewModel.Capacity - i - 1,
                        targetPoints[i].Value);
                }
                catch (Exception e)
                {
                    _logger.Error($"Ошибка при создании точки (FromCenterToBorder) {e.Message}");
                }
            }
            else
            {
                _logger.Info($"[SetData] Установка точки (Not FromCenterToBorder): {targetPoints[i].Value}");
                try
                {
                    _mainWindowViewModel.TargetSectionViewModel.SetPoint(
                        _mainWindowViewModel.TargetSectionViewModel.Capacity - targetPoints.Count + i,
                        targetPoints[i].Value);
                }
                catch (Exception e)
                {
                    _logger.Error($"Ошибка при создании точки (Not FromCenterToBorder) {e.Message}");
                }
            }
        }

        _logger.Info("[SetData] Обработка флагов и статусов");

        foreach (var t in data.Flags)
        foreach (var t2 in _mainWindowViewModel.StatusSectionViewModel.InfoStatusList)
            if (t2.Header == t.Name)
            {
                _logger.Info($"[SetData] Флаг {t.Name} -> {t.Value}");
                t2.Status = t.Value;
            }

        foreach (var t in data.Statuses)
        foreach (var t2 in _mainWindowViewModel.StatusSectionViewModel.InfoStatusList)
            if (t2.Header == t.Name)
            {
                _logger.Info($"[SetData] Статус {t.Name} -> {t.Value}");
                t2.Status = Convert.ToBoolean(t.Value);
            }

        _logger.Info("[SetData] Обработка параметров");

        foreach (var t in data.Parameters)
        foreach (var t2 in _mainWindowViewModel.InformationSectionViewModel.InfoBlockList)
            if (t2.Title == t.Name)
            {
                _logger.Info($"[SetData] Параметр {t.Name} -> {t.Value}");
                t2.Content = double.Round(t.Value, 2).ToString(CultureInfo.CurrentCulture);
            }

        _logger.Info("[SetData] Установка времени");
        _mainWindowViewModel.TargetSectionViewModel.SetTime(data);
    }
}