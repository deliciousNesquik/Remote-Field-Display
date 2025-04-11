using System.Net;
using NLog;
using NPFGEO.LWD.Net;
using RFD.Interfaces;

namespace RFD.Services;

public class ConnectionService: IConnectionService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly Client _client;
    
    public ConnectionService(Client client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        InitializeEvents();
    }
    
    public bool Connected => _client.Connected;
    public string Address => _client.Connected ? _client.Address.ToString() : "Нет подключения";
    private CancellationTokenSource _cancelTokenSource = new();
    private bool _needAutoReconnect = true;
    
    public event EventHandler<ReceiveDataEventArgs>? ReceiveData;
    public event EventHandler<ReceiveSettingsEventArgs>? ReceiveSettings;
    public event EventHandler? Disconnected;
    public event EventHandler? ConnectedStatusChanged;
    
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
                Logger.Info($"Подключение к серверу: {_client.Address}.");
                return true;
            }

            Logger.Error($"Ошибка подключения к серверу: {_client.Address}.");
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Ошибка подключения к серверу");
            return false;
        }
    }

    private async Task HandleDisconnection()
    {
        if (!_needAutoReconnect) return;

        _cancelTokenSource = new CancellationTokenSource();
        var token = _cancelTokenSource.Token;
        var address = _client.Address;

        Logger.Info("Попытка подключения к серверу пока клиент не подключится...");
        
        while (!token.IsCancellationRequested && !_client.Connected)
        {
            try
            {
                if (_client.Connected)
                    _client.Disconnect();

                _client.Address = address;
                if (_client.Connect())
                {
                    Logger.Info($"Подключение к серверу: {_client.Address}.");
                    break;
                }
                
                Logger.Error($"Ошибка подключения к серверу: {_client.Address}.");
                await Task.Delay(5000, token);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка при автоматическом переподключении");
                await Task.Delay(5000, token);
            }
        }
    }

    public async Task<bool> AutoConnectAsync()
    {
        _needAutoReconnect = false;
        await HandleDisconnection();
        _needAutoReconnect = true;

        return true;
    }

    public bool Disconnect()
    {
        _needAutoReconnect = false;
        _cancelTokenSource.Cancel();
        
        if (_client.Connected)
        {
            Logger.Info($"Отключение от сервера: {_client.Address}");
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
}