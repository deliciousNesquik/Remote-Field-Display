using System.Reactive;
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

        AutoConnection();
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

    private void AutoConnection()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _logger.Info("Запуск задачи автоматического подключения.");
        try
        {
            _connectionService.AutoConnectAsync();
        }
        catch (OperationCanceledException)
        {
            _logger.Warn("Операция автоматического подключения отменена.");
        }
        catch (Exception ex)
        {
            _logger.Error($"Не предвиденная ошибка при автоматическом подключении: {ex}");
        }

        Cancel();
    }

    private void Cancel()
    {
        _logger.Info("Окно автоматического подключения вызывает триггер для закрытия.");
        DialogClose?.Invoke(); //Вызывается для объекта который создал данное окно.
    }
}