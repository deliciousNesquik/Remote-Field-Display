using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NPFGEO.LWD.Net;
using RFD.Interfaces;
using RFD.Services;
using RFD.UserControls;
using RFD.ViewModels;
using RFD.Views;

namespace RFD;

internal sealed class Program
{
    public static IServiceProvider? Services { get; private set; }
    [STAThread]
    public static void Main(string[] args)
    {
        var logger = LogManager.Setup()
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
            LogManager.Shutdown();
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
        services.AddSingleton<IWindowService, WindowService>();
        
        services.AddSingleton<MainWindowViewModel>();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }
}