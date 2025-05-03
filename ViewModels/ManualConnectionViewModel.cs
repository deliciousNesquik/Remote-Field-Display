using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using RFD.Core;
using RFD.Interfaces;

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
                _logger.Error("Результат автоматического подключения не удачный.");
                _isBusy = false;
                return;
            }

            _isBusy = false;
            DialogClose?.Invoke(); //Вызывается для объекта который создал данное окно.

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
        _logger.Info($"Параметр _isBusy = {_isBusy}");
        _isBusy = false;
        _logger.Info("Окно ручного подключения вызывает триггер для закрытия.");
        DialogClose?.Invoke(); //Вызывается для объекта который создал данное окно.
    }
}