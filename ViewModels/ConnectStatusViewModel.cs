using Avalonia.Threading;
using ReactiveUI;
using RFD.Interfaces;

namespace RFD.ViewModels;

public class ConnectStatusViewModel : ViewModelBase
{
    private const string NotConnection = "Нет подключения";
    private readonly DispatcherTimer _disconnectTimer;
    private string _address;

    private bool _status;

    public ConnectStatusViewModel()
    {
        _status = false;
        _address = NotConnection;

        _disconnectTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _disconnectTimer.Tick += OnDisconnectTimerTick;
    }

    public bool Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public string Address
    {
        get => _address;
        set => this.RaiseAndSetIfChanged(ref _address, value);
    }

    private void UpdateConnectionStatus(string address, bool connected)
    {
        if (connected)
        {
            // Если восстановили соединение - сразу обновляем
            _disconnectTimer.Stop();
            _status = true;
            _address = address;
        }
        else if (_status)
        {
            // Если было подключение и потеряли - запускаем таймер
            if (!_disconnectTimer.IsEnabled) _disconnectTimer.Start();
        }
    }

    private void OnDisconnectTimerTick(object? sender, EventArgs e)
    {
        _disconnectTimer.Stop();
        _status = false;
        _address = NotConnection;
    }

    public void OnConnectionStateChanged(string address = NotConnection, bool connected = false)
    {
        UpdateConnectionStatus(address, connected);
    }
}