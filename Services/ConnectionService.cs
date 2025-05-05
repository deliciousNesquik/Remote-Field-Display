using System.Net;
using System.Text;
using NPFGEO.LWD.Net;
using RFD.Interfaces;

namespace RFD.Services;

public class ConnectionService : IConnectionService
{
    private readonly Client _client;
    private readonly ILoggerService _logger;
    private CancellationTokenSource _cancelTokenSource = new();
    private bool _needAutoReconnect = true;

    public ConnectionService(
        Client client,
        ILoggerService logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger;
        InitializeEvents();
    }

    public bool Connected => _client.Connected;
    public string Address => _client.Address.ToString();

    public event EventHandler<ReceiveDataEventArgs>? ReceiveData;
    public event EventHandler<ReceiveSettingsEventArgs>? ReceiveSettings;
    public event EventHandler? Disconnected;
    public event EventHandler? ConnectedStatusChanged;

    public async Task<bool> ConnectAsync(string address)
    {
        _needAutoReconnect = false;
        try
        {
            await _cancelTokenSource.CancelAsync();
            await Task.Delay(1000);

            if (_client.Connected)
                _client.Disconnect();

            _client.Address = IPAddress.Parse(address);
            _needAutoReconnect = true;

            if (_client.Connect())
            {
                _logger.Info($"Успешное подключение к: {_client.Address}.");
                return true;
            }

            _logger.Error($"Ошибка подключения к серверу: {_client.Address}.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка подключения к серверу");
            return false;
        }
    }

    public async Task<bool> AutoConnectAsync()
    {
        _needAutoReconnect = true;
        await HandleDisconnection();
        _needAutoReconnect = false;

        return true;
    }

    public bool Disconnect()
    {
        _needAutoReconnect = false;
        _cancelTokenSource.Cancel();

        if (_client.Connected)
        {
            _logger.Info($"Отключение от сервера: {_client.Address}");
            _client.Disconnect();

            return true;
        }

        return false;
    }

    public void Dispose()
    {
        Disconnect();
        _client.Dispose();
        _cancelTokenSource.Dispose();
    }

    private void InitializeEvents()
    {
        _client.ReceiveData += (s, e) => ReceiveData?.Invoke(s, e);
        _client.ReceiveSettings += (s, e) => ReceiveSettings?.Invoke(s, e);
        _client.Disconnected += (s, e) =>
        {
            Disconnected?.Invoke(s, e);
            HandleDisconnection().ConfigureAwait(false);
        };
        _client.ConnectedStatusChanged += (s, e) => ConnectedStatusChanged?.Invoke(s, e);
    }

    private async Task<bool> HandleDisconnection()
    {
        if (!_needAutoReconnect) return false;

        _cancelTokenSource = new CancellationTokenSource();
        var token = _cancelTokenSource.Token;
        if (_client.Address == null)
        {
            _logger.Error("Не удачная попытка подключения из-за отсутствия ip-адреса");
            _cancelTokenSource.Cancel();
            return false;
        }
        var address = _client.Address;

        _logger.Info("Попытка подключения к серверу пока клиент не подключится...");

        while (!token.IsCancellationRequested && !_client.Connected)
        {
            try
            {
                if (_client.Connected)
                    _client.Disconnect();

                _client.Address = address;
                if (_client.Connect())
                {
                    _logger.Info($"Успешное подключение к: {_client.Address}.");
                    break;
                }

                _logger.Error($"Ошибка подключения к серверу: {_client.Address}.");
                await Task.Delay(5000, token);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при автоматическом переподключении");
                await Task.Delay(5000, token);
            }
        }
        
        return true;
    }
}