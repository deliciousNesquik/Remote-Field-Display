using Avalonia.Threading;
using ReactiveUI;
using RFD.Interfaces;

namespace RFD.ViewModels;

public class ConnectStatusViewModel : ViewModelBase
{
    private const string NotConnection = "Нет подключения";
    private string _address;
    private bool _status;

    public ConnectStatusViewModel()
    {
        _status = false;
        _address = NotConnection;
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
        Status = connected;
        Address = address;
    }
    

    public void OnConnectionStateChanged(string address, bool connected)
    {
        if (!connected)
        {
            address = NotConnection;
        }
        UpdateConnectionStatus(address, connected);
    }
}