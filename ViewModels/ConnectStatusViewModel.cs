using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using ReactiveUI;
using RFD.Interfaces;

namespace RFD.ViewModels;

public class ConnectStatusViewModel : ViewModelBase
{
    private string _address = "";
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
            address = "";
        }
        else
        {
            address = "       " + address + "   ";
        }
        UpdateConnectionStatus(address, connected);
    }
}