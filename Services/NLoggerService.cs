using NLog;
using RFD.Interfaces;

namespace RFD.Services;

public class NLoggerService : ILoggerService
{
    private readonly Logger _logger;

    public NLoggerService()
    {
        _logger = LogManager.GetCurrentClassLogger();
    }

    public void Trace(string message) => _logger.Trace(message);
    public void Debug(string message) => _logger.Debug(message);
    public void Info(string message) => _logger.Info(message);
    public void Warn(string message) => _logger.Warn(message);
    public void Error(string message) => _logger.Error(message);
    public void Fatal(string message) => _logger.Fatal(message);
    public void Error(Exception ex, string message) => _logger.Error(ex, message);
    public void Fatal(Exception ex, string message) => _logger.Fatal(ex, message);
}