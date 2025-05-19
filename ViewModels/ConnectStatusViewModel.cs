using System.Net.Mime;
using Avalonia;
using Avalonia.Threading;
using ReactiveUI;
using RFD.Interfaces;

namespace RFD.ViewModels;

public class ConnectStatusViewModel : ViewModelBase
{
    private const string NotConnection = "✖";
    private string _address = NotConnection;
    private bool _status = false;

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