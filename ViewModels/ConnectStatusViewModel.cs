using Avalonia.Threading;
using ReactiveUI;
using RFD.Interfaces;

namespace RFD.ViewModels;

public class ConnectStatusViewModel: ViewModelBase
{
    private const string NOT_CONNECTION = "Нет подключения";
    private readonly DispatcherTimer _disconnectTimer;
    
    private bool _status;
    private string _address;
    
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

    public ConnectStatusViewModel()
    {
        _status = false;
        _address = NOT_CONNECTION;
        
        _disconnectTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _disconnectTimer.Tick += OnDisconnectTimerTick;
        
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
        _address = NOT_CONNECTION;
    }
    
    public void OnConnectionStateChanged(string address = NOT_CONNECTION, bool connected = false)
    {
        UpdateConnectionStatus(address, connected);
    }
}