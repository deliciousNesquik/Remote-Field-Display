using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReactiveUI;
using RFD.ViewModels;

namespace RFD.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainBorder.PointerPressed += MainBorder_PointerPressed;
            
            DataContext = new MainWindowViewModel();
        }

        private void WindowMinimizeButton_OnClick(object? sender, RoutedEventArgs e) { this.WindowState = WindowState.Minimized; }
        private void WindowMaximizeButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                MainBorder.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                MainBorder.Margin = new Thickness(0, 5, 0, 0);
            }
        }
        private void WindowCloseButton_OnClick(object? sender, RoutedEventArgs e) { this.Close(); }
        private void MainBorder_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Проверяем, что событие вызвано левой кнопкой мыши
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                // Инициируем перетаскивание окна
                BeginMoveDrag(e);
            }
        }
    }
}