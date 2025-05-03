using NPFGEO.LWD.Net;

namespace RFD.Interfaces;

public interface IConnectionService : IDisposable
{
    bool Connected { get; }
    string Address { get; }
    event EventHandler<ReceiveDataEventArgs> ReceiveData;
    event EventHandler<ReceiveSettingsEventArgs> ReceiveSettings;
    event EventHandler Disconnected;
    event EventHandler ConnectedStatusChanged;

    Task<bool> ConnectAsync(string address);
    Task<bool> AutoConnectAsync();
    bool Disconnect();
}