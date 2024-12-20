using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RFD.ViewModels;
using RFD.Views;

namespace RFD
{
    public partial class App : Application
    {
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



                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

    }
}