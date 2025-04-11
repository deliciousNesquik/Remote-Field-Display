using NPFGEO.LWD.Net;

namespace RFD.Interfaces;

public interface IBroadcastListener : IDisposable
{
    event EventHandler<ReceiveBroadcastEventArgs> ReceiveBroadcast;
    void Start();
    void Stop();
}