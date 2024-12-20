using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RFD.ViewModels;
using RFD.Views;
using NPFGEO.LWD.Net;

namespace RFD
{
    public partial class App : Application
    {
        ServerListener _listener;
        Client _client;
        bool _needAutoReconnect = true;
        public bool Connected { get => return _client.Connected; }
        public string CurrentIpAddress
        {
            get
            {
                if (_client.Address == null)
                    return string.Empty;
                else
                    return _client.Address.ToString();
            }
        }
        CancellationTokenSource _cancelTokenSource;
        CancellationToken _token;
        
        
        public override void Initialize()
        {
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
                
                
                //TODO
                //Сделать полное подключение к серверу
                mainWindowViewModel.IpAddress = CurrentIpAddress;


                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

    }
}