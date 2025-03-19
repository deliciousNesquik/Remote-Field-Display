using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NPFGEO.LWD.Net;

public class NetworkService : IDisposable
{
    public Client Client { get; private set; }
    public ServerListener ServerListener { get; private set; }
    private CancellationTokenSource _cancelTokenSource = new();
    private bool _needAutoReconnect = true;

    // Кэш для данных
    private DataObject _dataObj = new DataObject();
    
    public event Action<Settings>? SettingsReceived;
    public event Action<DataObject>? DataReceived;
    public event Action? Disconnected;
    public event Action? ConnectedStatusChanged;

    public NetworkService()
    {
        Client = new Client();
        ServerListener = new ServerListener();
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        Client.ReceiveData += Client_ReceiveData;
        Client.ReceiveSettings += Client_ReceiveSettings;
        Client.Disconnected += Client_Disconnected;
        Client.ConnectedStatusChanged += Client_ConnectedStatusChanged;
        ServerListener.ReceiveBroadcast += ServerListenerReceiveBroadcast;
    }

    public bool Connect(string address)
    {
        _needAutoReconnect = false;
        try
        {
            ServerListener.Stop();
            _cancelTokenSource.CancelAsync();
            Task.Delay(1000);

            if (Client.Connected)
                Client.Disconnect();

            Client.Address = IPAddress.Parse(address);
            Client.Connect();
            _needAutoReconnect = true;
            return true;
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            return false;
        }
    }

    public void Disconnect()
    {
        _needAutoReconnect = false;
        ServerListener.Stop();

        if (Client.Connected)
            Client.Disconnect();
    }

    /// <summary>
    /// Выполняет автоматическое переподключение.
    /// </summary>
    /// <returns>Значение <c>true</c>, если подключение успешно, иначе <c>false</c>.</returns>
    private async Task<bool> AutoConnectAsync()
    {
        try
        {
            // Останавливаем слушатель входящих соединений
            ServerListener.Stop();

            // Отменяем текущий токен
            await _cancelTokenSource.CancelAsync();

            // Задержка перед повторным подключением
            await Task.Delay(2500, _cancelTokenSource.Token);

            // Если клиент всё ещё подключен, разрываем соединение
            if (Client.Connected)
                Client.Disconnect();

            // Запускаем слушатель снова
            ServerListener.Start();

            return true; // Успешное выполнение
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            return false; // Ошибка при выполнении
        }
    }
    
    /// <summary>
    /// Инициирует процесс автоматического переподключения.
    /// </summary>
    /// <returns>Значение <c>true</c>, если подключение успешно, иначе <c>false</c>.</returns>
    public async Task<bool> AutoConnect()
    {
        _needAutoReconnect = false; // Отключаем авто-переподключение перед началом процесса

        bool result = await AutoConnectAsync(); // Запускаем процесс переподключения

        _needAutoReconnect = true; // После завершения снова включаем авто-переподключение

        return result; // Возвращаем результат переподключения
    }

    private void Client_ReceiveData(object? sender, ReceiveDataEventArgs e)
    {
        // Обновляем кэш данных
        _dataObj = DataObject.Union(_dataObj, e.Data);

        // Вызываем событие с обновленными данными
        DataReceived?.Invoke(_dataObj);
    }

    private void Client_ReceiveSettings(object? sender, ReceiveSettingsEventArgs e)
    {
        SettingsReceived?.Invoke(e.Settings);
    }

    private void Client_Disconnected(object? sender, EventArgs e)
    {
        Disconnected?.Invoke();
    }

    private void Client_ConnectedStatusChanged(object? sender, EventArgs e)
    {
        ConnectedStatusChanged?.Invoke();
    }

    private void ServerListenerReceiveBroadcast(object? sender, ReceiveBroadcastEventArgs e)
    {
        if (Client.Connected) return;

        ServerListener.Stop();
        Client.Address = e.Server.Address;
        Client.Connect();
    }

    public void ClearCache()
    {
        _dataObj = new DataObject();
    }

    public void Dispose()
    {
        Client.Dispose();
        //_listener.Dispose(); данные класс не реализует Dispose :(
    }
}