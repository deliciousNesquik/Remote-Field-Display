using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RFD.ViewModels;
using RFD.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Avalonia.Threading;
using NPFGEO.LWD.Net;

namespace RFD
{
    public partial class App : Application
    {
        public static event Action? ConnectionUpdated;
        public static event Action<ReceiveSettingsEventArgs> SettingsUpdated;
        
        //Отключение логики подключения, чтобы работать с интерфейсом
        /*
        private Client _client;
        private ServerListener _listener;
        private bool _needAutoReconnect = true;
        

        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _token;
        */

        public bool Connected => false;
        public string CurrentIpAddress => "Нет подключения";

        public override void Initialize()
        {
            //TODO
            //Сделать обработку тут
            
            /*
             
            Unhandled exception. System.Net.Sockets.SocketException (10048): ������ ����������� ������ ���� ������������� ������ ������ (��������/������� �����/����).
            at System.Net.Sockets.Socket.UpdateStatusAfterSocketErrorAndThrowException(SocketError error, Boolean disconnectOnFailure, String callerName)
            at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
            at System.Net.Sockets.Socket.Bind(EndPoint localEP)
            at NPFGEO.LWD.Net.ServerListener.InitializeUdpClient() in D:\Programming\Study\��������� ������\NPFGEO\LWD\NPFGEO.LWD.Net\ServerListener.cs:line 78
            at NPFGEO.LWD.Net.ServerListener.Start() in D:\Programming\Study\��������� ������\NPFGEO\LWD\NPFGEO.LWD.Net\ServerListener.cs:line 71
            at RFD.App.Initialize() in D:\Programming\Study\��������� ������\NPFGEO\LWD\RFD\App.axaml.cs:line 56
            at Avalonia.AppBuilder.SetupUnsafe()
            at Avalonia.AppBuilder.Setup()
            at Avalonia.AppBuilder.SetupWithLifetime(IApplicationLifetime lifetime)
            at Avalonia.ClassicDesktopStyleApplicationLifetimeExtensions.StartWithClassicDesktopLifetime(AppBuilder builder, String[] args, Action`1 lifetimeBuilder)
            at RFD.Program.Main(String[] args) in D:\Programming\Study\��������� ������\NPFGEO\LWD\RFD\Program.cs:line 13
             */
            
            
            /*_client = new Client();
            _client.ReceiveData += Client_ReceiveData;
            _client.ReceiveSettings += Client_ReceiveSettings;
            _client.Disconnected += Client_Disconnected;
            _client.ConnectedStatusChanged += _client_ConnectedStatusChanged;

            _listener = new ServerListener();
            _listener.ReceiveBroadcast += Listener_ReceiveBroadcast;
            _listener.Start();*/
            
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
                MainWindow mainWindow = new MainWindow()
                {
                    DataContext = mainWindowViewModel,
                };
                
                desktop.MainWindow = mainWindow;
                
                mainWindowViewModel.OpenAutomaticConnecting();
            }

            base.OnFrameworkInitializationCompleted();
        }
        
        /*
        void Listener_ReceiveBroadcast(object sender, ReceiveBroadcastEventArgs e)
        {
            if (_client != null && _client.Connected) return;

            _listener.Stop();

            _client.Address = e.Server.Address;
            _client.Connect();
            ConnectionUpdated?.Invoke();
            Console.WriteLine("Connected to " + _client.Address.ToString());
        }

        private void Client_ReceiveSettings(object sender, ReceiveSettingsEventArgs e)
        {
            Console.WriteLine("SettingsUpdated");
            Action action = () =>
            {
                Console.WriteLine("SettingsUpdatedAction");
                SettingsUpdated?.Invoke(e);
            };
            action();
            //Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, action);
            Dispatcher.UIThread.Post(action, Avalonia.Threading.DispatcherPriority.Background);
        }
        
        private readonly object _settingsLock = new object();

        private void Client_ReceiveSettings(object sender, ReceiveSettingsEventArgs e)
        {
            Console.WriteLine("SettingsUpdated");
            Action action = () =>
            {
                Console.WriteLine("SettingsUpdatedAction");
                lock (_settingsLock)
                {
                    SettingsUpdated?.Invoke(e);
                }
            };
            action();
            Dispatcher.UIThread.Post(action, Avalonia.Threading.DispatcherPriority.Background);
        }

        private void Client_ReceiveData(object sender, ReceiveDataEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                NavigationPanelVM.Current.SetData(e.Data);
            });
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected");
            ConnectionUpdated?.Invoke();
            //ConnectionControlViewModel.Current.Update();
            if (_needAutoReconnect)
            {
                _cancelTokenSource = new CancellationTokenSource();
                _token = _cancelTokenSource.Token;
                Action action = new Action(() =>
                {
                    var address = _client.Address;
                    while (!_token.IsCancellationRequested && !_client.Connected)
                    {
                        Reconnect(address);
                        ConnectionUpdated?.Invoke();
                        //ConnectionControlViewModel.Current.Update();
                    }
                });
                Task.Factory.StartNew(action, _token);
            }
        }

        private void _client_ConnectedStatusChanged(object sender, EventArgs e)
        {
            ConnectionUpdated?.Invoke();
            //ConnectionControlViewModel.Current.Update();
        }

        private Task AutoConnectAsync()
        {
            var task = Task.Factory.StartNew(() =>
            {
                _listener.Stop();
                _cancelTokenSource?.Cancel();

                System.Threading.Thread.Sleep(2500);

                if (_client != null && _client.Connected)
                    _client.Disconnect();

                _listener.Start();
            });
            return task;
        }

        public async void AutoConnect()
        {
            _needAutoReconnect = false;
            var autoConnect = AutoConnectAsync();
            await autoConnect;
            _needAutoReconnect = true;
        }

        public bool Connect(string address)
        {
            _needAutoReconnect = false;
            try
            {
                _listener.Stop();

                _cancelTokenSource?.Cancel();
                System.Threading.Thread.Sleep(1000);

                if (_client != null && _client.Connected)
                    _client.Disconnect();

                _client.Address = System.Net.IPAddress.Parse(address);
                _client.Connect();
                Console.WriteLine("Connected to " + _client.Address.ToString());
                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                Console.WriteLine("Подключение не удалось");
                return false;
            }
            _needAutoReconnect = true;
        }

        private void Reconnect(IPAddress address)
        {
            _needAutoReconnect = false;
            try
            {
                _listener.Stop();

                if (_client != null && _client.Connected)
                    _client.Disconnect();

                _client.Address = address;
                _client.Connect();
                //Logger.Info("Connected to " + _client.Address.ToString());
            }
            catch (Exception exc)
            {
            } //Logger.Error(exc); }

            _needAutoReconnect = true;
        }

        public void Disconnect()
        {
            //TODO
            //Реализовать нормальное отключение пользователя от ip-адреса
            //чтобы при повторном подключении не вылетала ошибка что порт уже занят
            
            _needAutoReconnect = false;
            _listener.Stop();

            if (_client != null && _client.Connected)
            {
                Console.WriteLine("Application disconnect to server " + "{_client.Connected: " + _client.Connected + "}");
                _client.Disconnect();
            }
                
        }*/
        
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}