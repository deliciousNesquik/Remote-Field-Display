using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using NPFGEO.LWD.Net;
using RFD.Interfaces;
using RFD.Models;
using RFD.Services;
using RFD.ViewModels;
using RFD.Views;

namespace RFD;

public class App : Application
{
    private IBroadcastListener? _broadcastListener;
    private IConnectionService? _connectionService;
    private DataObject _dataObj = new();
    private ILoggerService? _logger;
    private MainWindowViewModel? _mainWindowViewModel;

    public override void Initialize()
    {
        Current!.Name = "RFD";
        
        AvaloniaXamlLoader.Load(this);

        if (Design.IsDesignMode)
        {
            Console.WriteLine("Приложение запущено в режиме Design-time");
            return;
        }

        _logger = Program.Services!.GetRequiredService<ILoggerService>();
        _logger.Info("Инициализация приложения");

        try
        {
            _connectionService = Program.Services!.GetRequiredService<IConnectionService>();
            _broadcastListener = Program.Services!.GetRequiredService<IBroadcastListener>();

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
        if (_connectionService == null || _broadcastListener == null) return;

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
            SettingsApplication.Load();
            
            SettingsViewModel.ChangeBrandColor(SettingsApplication.BrandColor);
            LocalizationManager.ApplyLocalization(SettingsApplication.LanguageLocalization);
            

            _mainWindowViewModel = Program.Services!.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow()
            {
                DataContext = _mainWindowViewModel
            };
            desktop.Exit += (_, __) =>
            {
                _connectionService?.Dispose();
                _broadcastListener?.Dispose();
                GC.SuppressFinalize(this);
                Environment.Exit(0);
            };
        }

        base.OnFrameworkInitializationCompleted();
    }


    private void Listener_ReceiveBroadcast(object? sender, ReceiveBroadcastEventArgs e)
    {
        if (_logger == null || _connectionService == null || _broadcastListener == null ||
            _mainWindowViewModel == null || _mainWindowViewModel.ConnectStatusViewModel == null) return;

        _logger.Info($"Получение широковещательного сигнала от {e.Server.Address}:{e.Server.Port}");
        if (_connectionService.Connected)
        {
            _logger.Info("Отклонение сигнала. Приложение уже было подключено.");
            return;
        }

        _logger.Info("Остановка прослушивания BroadcastListener");
        _broadcastListener.Stop();

        if (_connectionService.ConnectAsync(e.Server.Address.ToString()).Result)
        {
            _logger.Info($"Подключение успешно к: {_connectionService.Address}:{e.Server.Port}");
        }
        else
        {
            _logger.Error($"Ошибка при попытке подключения к: {_connectionService.Address}");
        }
        _mainWindowViewModel.ConnectStatusViewModel.OnConnectionStateChanged(_connectionService.Address, _connectionService.Connected);
    }

    private void Client_ReceiveSettings(object? sender, ReceiveSettingsEventArgs e)
    {
        if (_logger == null || _connectionService == null) return;

        _logger.Info($"Получение настроек приложения от: {_connectionService.Address}");
        try
        {
            Dispatcher.UIThread.InvokeAsync(() => { SetSettings(e.Settings); }, DispatcherPriority.Background);
        }
        catch (Exception ex)
        {
            _logger.Error($"Ошибка при запуске background потока обработки настроек приложения: {ex}");
        }
    }

    private void Client_ReceiveData(object? sender, ReceiveDataEventArgs e)
    {
        if (_logger == null || _connectionService == null) return;

        _logger.Info($"Получение данных приложения от: {_connectionService.Address}");
        try
        {
            Dispatcher.UIThread.InvokeAsync(() => { SetData(e.Data); });
        }
        catch (Exception ex)
        {
            _logger.Error($"Ошибка при запуске background потока обработки данных приложения: {ex}");
        }
    }

    private void Client_Disconnected(object? sender, EventArgs e)
    {
        if (_logger == null || _mainWindowViewModel == null || _connectionService == null ||
            _mainWindowViewModel.ConnectStatusViewModel == null) return;

        _logger.Warn($"Разорвано соединение с: {_connectionService.Address}");
        _mainWindowViewModel.ConnectStatusViewModel.OnConnectionStateChanged(_connectionService.Address,
            _connectionService.Connected);
    }

    private void Client_ConnectedStatusChanged(object? sender, EventArgs e)
    {
        if (_logger == null || _mainWindowViewModel == null || _connectionService == null ||
            _mainWindowViewModel.ConnectStatusViewModel == null) return;

        _logger.Info(
            $"Статус подключения для: {_connectionService.Address} изменен: {!_connectionService.Connected} -> {_connectionService.Connected}");
        _mainWindowViewModel.ConnectStatusViewModel.OnConnectionStateChanged(_connectionService.Address,
            _connectionService.Connected);
    }

    private void SetSettings(NPFGEO.LWD.Net.Settings settings)
    {
        if (_mainWindowViewModel == null) return;
        
        ApplyStatusesSettings(settings);
        ApplyInformationSettings(settings);
        ApplyTargetParametersSettings(settings);

        // Проверка на защиту от получения темы приложения.
        if (!SettingsApplication.ThemeProtection)
            ThemeManager.ApplyTheme(settings.ThemeStyle == "LightTheme" ? AppTheme.Light : AppTheme.Dark);

        

        // Использование кеша для хранения последних данных.
        SetData(_dataObj);
    }

    private void ApplyTargetParametersSettings(Settings settings)
    {
        if (_mainWindowViewModel == null) return;
        
        _mainWindowViewModel.TargetSectionViewModel.MagneticDeclination = settings.InfoParameters.MagneticDeclination;
        _mainWindowViewModel.TargetSectionViewModel.ToolfaceOffset = settings.InfoParameters.ToolfaceOffset;
        _mainWindowViewModel.TargetSectionViewModel.FromCenterToBorder = settings.Target.FromCenterToBorder;
        _mainWindowViewModel.TargetSectionViewModel.Capacity = settings.Target.Capacity;
        _mainWindowViewModel.TargetSectionViewModel.IsHalfMode = settings.Target.IsHalfMode;
        _mainWindowViewModel.TargetSectionViewModel.GridFrequency = settings.Target.GridFrequency;
        _mainWindowViewModel.TargetSectionViewModel.FontSize = settings.Target.FontSize;
        (_mainWindowViewModel.TargetSectionViewModel.RingWidth, _mainWindowViewModel.TargetSectionViewModel.RingThickness) = (settings.Target.RingWidth, settings.Target.RingWidth);
        _mainWindowViewModel.TargetSectionViewModel.DefaultRadius = settings.Target.DefaultRadius;
        _mainWindowViewModel.TargetSectionViewModel.ReductionFactor = settings.Target.ReductionFactor;
        
        // Установка сектора бурения. 
        _mainWindowViewModel.TargetSectionViewModel.SetSector(
            settings.Target.SectorDirection - settings.Target.SectorWidth / 2,
            settings.Target.SectorDirection + settings.Target.SectorWidth / 2
        );
    }

    private void ApplyInformationSettings(Settings settings)
    {
        if (_mainWindowViewModel == null) return;
        
        // Объединение всех параметров в единый список и формирование множества имен для быстрого поиска
        var allParameters = settings.Parameters.ToList();
        var allParameterNames = new HashSet<string>(allParameters.Select(p => p.Name));

        var currentParameters = _mainWindowViewModel.InformationSectionViewModel.InfoBlockList.ToList();

        // Добавляем недостающие параметры
        foreach (var param in allParameters.Where(param => currentParameters.All(p => p.Title != param.Name)))
        {
            _mainWindowViewModel.InformationSectionViewModel.AddInfoBox(new InfoBox(param.Name, "-", param.Units));
        }

        // Удаляем параметры, которые больше не передаются
        foreach (var param in currentParameters.Where(p => !allParameterNames.Contains(p.Title)))
        {
            _mainWindowViewModel.InformationSectionViewModel.ClearInfoBox(param.Title);
        }
    }

    private void ApplyStatusesSettings(Settings settings)
    {
        if (_mainWindowViewModel == null) return;
        
        // Объединение всех статусов в единое целое.
        // Проверка каждого статуса на его существование в списке, если статус есть,
        // тогда стоит обновить только значение, в противном случае добавить в конец списка данный статус.
        var allStatuses = settings.Statuses.Concat(settings.Flags).ToList();
        var allStatusNames = new HashSet<string>(allStatuses.Select(s => s.Name)); // Для быстрого поиска
        var currentStatuses = _mainWindowViewModel.StatusSectionViewModel.InfoStatusList.ToList();

        foreach (var status in allStatuses.Where(status => currentStatuses.All(s => s.Header != status.Name)))
        {
            // В случае когда статус не найден в уже существующем списке
            // тогда необходимо добавить новый в конец.
            _mainWindowViewModel.StatusSectionViewModel.AddStatusBox(new StatusBox(status.Name));
        }
        
        // В случае когда статус убран из передачи,
        // его необходимо аккуратно удалить из пользовательского интерфейса.
        foreach (var status in currentStatuses.Where(status => !allStatusNames.Contains(status.Header)))
        {
            _mainWindowViewModel.StatusSectionViewModel.ClearStatusBox(status.Header);
        }
    }

    private void SetData(DataObject data)
    {
        if (_mainWindowViewModel == null || _logger == null) return;

        _dataObj = DataObject.Union(_dataObj, data);

        var targetPoints = _dataObj.TargetPoints.ToList();
        for (var i = 0; i < targetPoints.Count; i++)
        {
            _mainWindowViewModel.TargetSectionViewModel.Angle = Math.Round(targetPoints[i].Angle, 2);
            _mainWindowViewModel.TargetSectionViewModel.ToolfaceType = targetPoints[i].ToolfaceType.ToString()[..1];

            if (_mainWindowViewModel.TargetSectionViewModel.FromCenterToBorder)
                try
                {
                    _mainWindowViewModel.TargetSectionViewModel.SetPoint(_mainWindowViewModel.TargetSectionViewModel.Capacity - i - 1, targetPoints[i].Value);
                }
                catch (Exception e)
                {
                    _logger.Error($"Ошибка при создании точки (FromCenterToBorder) {e.Message}");
                }
            else
                try
                {
                    _mainWindowViewModel.TargetSectionViewModel.SetPoint(_mainWindowViewModel.TargetSectionViewModel.Capacity - targetPoints.Count + i, targetPoints[i].Value);
                }
                catch (Exception e)
                {
                    _logger.Error($"Ошибка при создании точки (Not FromCenterToBorder) {e.Message}");
                }
        }

        foreach (var t in _dataObj.Flags)
        foreach (var t2 in _mainWindowViewModel.StatusSectionViewModel.InfoStatusList)
            if (t2.Header == t.Name)
                t2.Status = t.Value;

        foreach (var t in _dataObj.Statuses)
        foreach (var t2 in _mainWindowViewModel.StatusSectionViewModel.InfoStatusList)
            if (t2.Header == t.Name)
                t2.Status = Convert.ToBoolean(t.Value);

        foreach (var t in _dataObj.Parameters)
        foreach (var t2 in _mainWindowViewModel.InformationSectionViewModel.InfoBlockList)
            if (t2.Title == t.Name)
                t2.Content = double.Round(t.Value, 2).ToString(CultureInfo.CurrentCulture);
        
        
        _mainWindowViewModel.TargetSectionViewModel.SetTime(_dataObj);
    }
}