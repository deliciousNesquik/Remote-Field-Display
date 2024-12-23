using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RFD.ViewModels;
using RFD.Views;
using Net;
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

namespace RFD
{
    public partial class App : Application
    {
        public static event Action<bool> ConnectionUpdated;
        public static event Action<bool> AutomaticConnect;
        
        private Client _client;
        private ServerListener _listener;
        private bool _needAutoReconnect = true;
        public bool Connected => _client.Connected;

        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _token;
        
        public string CurrentIpAddress
        {
            get
            {
                if (_client.Address == null)
                    return "Не найдено";
                else
                    return _client.Address.ToString();
            }
        }
        public override void Initialize()
        {
            _client = new Client();
            _client.ReceiveData += Client_ReceiveData;
            _client.ReceiveSettings += Client_ReceiveSettings;
            _client.Disconnected += Client_Disconnected;
            _client.ConnectedStatusChanged += _client_ConnectedStatusChanged;

            _listener = new ServerListener();
            _listener.ReceiveBroadcast += Listener_ReceiveBroadcast;
            _listener.Start();
            
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
            }

            base.OnFrameworkInitializationCompleted();
        }
        
        void Listener_ReceiveBroadcast(object sender, ReceiveBroadcastEventArgs e)
        {
            if (_client != null && _client.Connected) return;

            _listener.Stop();

            _client.Address = e.Server.Address;
            _client.Connect();
            //ConnectionControlViewModel.Current.Update();
            ConnectionUpdated?.Invoke(true);
            Console.WriteLine("Connected to " + _client.Address.ToString());
        }

        private void Client_ReceiveSettings(object sender, ReceiveSettingsEventArgs e)
        {
            Action action = () =>
            {
                //NavigationPanelVM.Current.SetSettings(e.Settings);
            };
            //Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, action);
        }

        private void Client_ReceiveData(object sender, ReceiveDataEventArgs e)
        {
            /*Dispatcher.Invoke(() =>
            {
                NavigationPanelVM.Current.SetData(e.Data);
            });*/
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected");
            ConnectionUpdated?.Invoke(true);
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
                        ConnectionUpdated?.Invoke(true);
                        //ConnectionControlViewModel.Current.Update();
                    }
                });
                Task.Factory.StartNew(action, _token);
            }
        }

        private void _client_ConnectedStatusChanged(object sender, EventArgs e)
        {
            ConnectionUpdated?.Invoke(true);
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
                
        }
        
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}