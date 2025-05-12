using System.Reactive;
using Avalonia.Threading;
using NPFGEO.LWD.Net;
using ReactiveUI;
using RFD.Interfaces;
using RFD.Services;

namespace RFD.ViewModels;

public class AutomaticConnectionDialogViewModel : ViewModelBase, IDialog
{
    private readonly IConnectionService _connectionService;
    private readonly ILoggerService _logger;

    private CancellationTokenSource? _cancellationTokenSource;


    public AutomaticConnectionDialogViewModel()
    {
        _logger = new NLoggerService();
        _connectionService = new ConnectionService(new Client(), _logger);


        ConfirmCommand = null!;
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    public AutomaticConnectionDialogViewModel(
        IConnectionService connectionService,
        ILoggerService loggerService)
    {
        _connectionService = connectionService;
        _logger = loggerService;

        ConfirmCommand = null!;
        CancelCommand = ReactiveCommand.Create(Cancel);

        AutoConnection();
    }

    public ReactiveCommand<Unit, Unit> ConfirmCommand { get; set; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }
    public Action? DialogClose { get; set; }

    private async void AutoConnection()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _logger.Info("Запуск задачи автоматического подключения.");

        var connect = Task.Run(() => _connectionService.AutoConnectAsync());
    
        if (!connect.Result)
        {
            _logger.Error("Результат автоматического подключения не удачный.");
            return;
        }
     
        
        await Task.Delay(2000);
        Dispatcher.UIThread.Post(() =>
        {
            DialogClose?.Invoke();
            Cancel();
        });
    }

    private void Cancel()
    {
        _logger.Info($"Окно автоматического подключения вызывает триггер для закрытия в {this}");
        _cancellationTokenSource?.Cancel();
        DialogClose?.Invoke(); //Вызывается для объекта который создал данное окно.
    }
}