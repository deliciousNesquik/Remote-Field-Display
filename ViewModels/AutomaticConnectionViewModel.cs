using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using RFD.Interfaces;

namespace RFD.ViewModels;

public class AutomaticConnectionDialogViewModel: ViewModelBase, IDialog
{
    private readonly IConnectionService _connectionService;
    private readonly ILoggerService _logger;
    
    public IRelayCommand ConfirmCommand { get; set; }
    public IRelayCommand CancelCommand { get; set; }
    public Action? DialogClose { get; set; }

    private CancellationTokenSource? _cancellationTokenSource;


    public AutomaticConnectionDialogViewModel()
    {
        ConfirmCommand = null!;
        CancelCommand = new RelayCommand(Cancel);
        
        AutoConnection();
    }

    public AutomaticConnectionDialogViewModel(
        IConnectionService connectionService,
        ILoggerService loggerService)
    {
        _connectionService = connectionService;
        _logger = loggerService;

        ConfirmCommand = null!;
        CancelCommand = new RelayCommand(Cancel);
        
        AutoConnection();
    }

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