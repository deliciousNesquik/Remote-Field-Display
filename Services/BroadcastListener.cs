using NLog;
using NPFGEO.LWD.Net;
using RFD.Interfaces;

namespace RFD.Services;

public class BroadcastListener : IBroadcastListener
{
    private static ILoggerService? _logger;
    private readonly ServerListener _listener;

    public BroadcastListener(ServerListener listener, ILoggerService logger)
    {
        _logger = logger;
        _listener = listener ?? throw new ArgumentNullException(nameof(listener));
        _listener.ReceiveBroadcast += (s, e) => ReceiveBroadcast?.Invoke(s, e);
    }

    public event EventHandler<ReceiveBroadcastEventArgs> ReceiveBroadcast;

    public void Start()
    {
        _logger.Info("Старт ServerListener для прослушивания сервера.");
        _listener.Start();
    }

    public void Stop()
    {
        _logger.Info("Остановка прослушивания ServerListener.");
        _listener.Stop();
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
        _logger.Info("Высвобождение ресурсов для прослушивания сервера");
    }
}