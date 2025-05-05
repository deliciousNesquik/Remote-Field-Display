using System.Reactive;
using System.Reactive.Linq;
using NPFGEO.LWD.Net;
using ReactiveUI;
using RFD.Core;
using RFD.Interfaces;
using RFD.Services;

namespace RFD.ViewModels;

public class ManualConnectionDialogViewModel : ViewModelBase, IDialog
{
    private readonly IConnectionService _connectionService;
    private readonly ILoggerService _logger;
    private string _address = "";

    private bool _isBusy;

    public ManualConnectionDialogViewModel(
        IConnectionService connectionService,
        ILoggerService loggerService)
    {
        _connectionService = connectionService;
        _logger = loggerService;

        ConfirmCommand = ReactiveCommand.Create(Confirm);
        CancelCommand = ReactiveCommand.Create(Cancel, Observable.Return(!_isBusy));
    }

    public ManualConnectionDialogViewModel()
    {
        _logger = new NLoggerService();
        _connectionService = new ConnectionService(new Client(), _logger);
        
        ConfirmCommand = ReactiveCommand.Create(Confirm);
        CancelCommand = ReactiveCommand.Create(Cancel, Observable.Return(!_isBusy));
    }

    public string Аddress
    {
        get => _address;
        set => this.RaiseAndSetIfChanged(ref _address, value);
    }

    public ReactiveCommand<Unit, Unit> ConfirmCommand { get; set; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }
    public Action? DialogClose { get; set; }

    private void Confirm()
    {
        _logger.Debug($"Проверка ip адреса: {_address}");
        if (IpAddressValidator.IsValidIPv4(_address))
        {
            _logger.Info("Успешно проверен адрес. Попытка подключения по данному адресу.");

            _isBusy = true;

            var connect = Task.Run(() => _connectionService.ConnectAsync(_address));
            if (!connect.Result)
            {
                _logger.Error("Результат ручного подключения не удачный.");
                _isBusy = false;
                return;
            }

            _isBusy = false;
            Cancel();
        }
        else
        {
            _isBusy = false;
            _logger.Error("Ошибка не правильно написан адрес");
        }
    }

    private void Cancel()
    {
        _isBusy = false;
        _logger.Info("Окно ручного подключения вызывает триггер для закрытия.");
        DialogClose?.Invoke(); //Вызывается для объекта который создал данное окно.
    }
}