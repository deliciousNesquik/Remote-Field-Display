using System;
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
        private const int DragRegionHeight = 30; // Высота области для перетаскивания

        public MainWindow()
        {
            InitializeComponent();
            MainBorder.PointerPressed += MainBorder_PointerPressed;

            DataContext = new MainWindowViewModel();
            
            // Подписываемся на событие изменения состояния окна
            this.GetObservable(WindowStateProperty).Subscribe(state =>
            {
                if (state == WindowState.Maximized)
                {
                    MainBorder.Margin = new Thickness(7, 7, 7, 7); // Добавляем отступы
                }
                else
                {
                    MainBorder.Margin = new Thickness(0, 0, 0, 0); // Убираем отступы
                }
            });
        }

        private void WindowMinimizeButton_OnClick(object? sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

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
                MainBorder.Margin = new Thickness(7, 7, 7, 7);
            }
        }

        private void WindowCloseButton_OnClick(object? sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MainBorder_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Проверяем, что событие вызвано левой кнопкой мыши
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                // Получаем координаты точки нажатия относительно окна
                var position = e.GetPosition(MainBorder);

                // Проверяем, что точка находится в верхней части области MainBorder
                if (position.Y <= DragRegionHeight)
                {
                    // Если окно в полноэкранном режиме, возвращаем его в нормальный режим перед началом перетаскивания
                    if (this.WindowState == WindowState.Maximized)
                    {
                        this.WindowState = WindowState.Normal;

                        // Убираем отступы
                        MainBorder.Margin = new Thickness(0, 0, 0, 0);

                        // Пересчитываем координаты точки нажатия относительно нового размера окна
                        var screenPosition = e.GetPosition(this);
                        var newLeft = screenPosition.X - (this.Bounds.Width / 2);
                        var newTop = screenPosition.Y - DragRegionHeight;

                        this.Position = new PixelPoint((int)newLeft, (int)newTop);
                    }

                    // Инициируем перетаскивание окна
                    BeginMoveDrag(e);
                }
            }
        }
    }
}