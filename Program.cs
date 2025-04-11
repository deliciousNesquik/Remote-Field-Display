using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NPFGEO.LWD.Net;
using RFD.Interfaces;
using RFD.Services;
using RFD.ViewModels;
using RFD.Views;

namespace RFD;

internal sealed class Program
{
    // Инициализация DI контейнера
    public static IServiceProvider? Services { get; private set; }
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Настройка NLog
        var logger = NLog.LogManager.Setup()
            .LoadConfigurationFromFile("NLog.config")
            .GetCurrentClassLogger();
            
        try
        {
            logger.Debug("Инициализация приложения");
            
            // Создаем DI контейнер
            var services = new ServiceCollection();
            
            // Конфигурация сервисов
            ConfigureServices(services);
            
            // Строим провайдер сервисов
            Services = services.BuildServiceProvider();
            
            // Запуск Avalonia
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Необработанное исключение при запуске");
            throw;
        }
        finally
        {
            NLog.LogManager.Shutdown();
        }
    }
    
    // Конфигурация DI сервисов
    private static void ConfigureServices(IServiceCollection services)
    {
        // 1. Сначала регистрируем зависимости нижнего уровня
        services.AddSingleton<Client>();
        services.AddSingleton<ServerListener>();
        
        // 2. Затем сервисы, которые зависят от Client
        services.AddSingleton<IConnectionService, ConnectionService>();
        services.AddSingleton<IBroadcastListener, BroadcastListener>();
        
        // 3. Регистрируем логгер
        services.AddSingleton<ILoggerService, NLoggerService>();
        
        // 4. Регистрируем ViewModel и другие компоненты
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<MainWindow>();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }
}