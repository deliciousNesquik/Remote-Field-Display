using NLog;
using NPFGEO.LWD.Net;
using RFD.Interfaces;

namespace RFD.Services;

public class BroadcastListener : IBroadcastListener
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ServerListener _listener;

    public BroadcastListener(ServerListener listener)
    {
        _listener = listener ?? throw new ArgumentNullException(nameof(listener));
        _listener.ReceiveBroadcast += (s, e) => ReceiveBroadcast?.Invoke(s, e);
    }

    public event EventHandler<ReceiveBroadcastEventArgs> ReceiveBroadcast;

    public void Start()
    {
        Logger.Info("Старт ServerListener для прослушивания сервера.");
        _listener.Start();
    }

    public void Stop()
    {
        Logger.Info("Остановка прослушивания ServerListener.");
        _listener.Stop();
    }

    public void Dispose()
    {
        Stop();
    }
}