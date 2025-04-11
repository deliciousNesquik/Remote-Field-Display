using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using RFD.ViewModels;
using static System.Environment;

namespace RFD.Views;

public partial class MainWindow : Window
{
    /// <summary>
    ///     Поле получающее значение высоты верхней панели управления приложением
    /// </summary>
    private readonly double _dragRegionHeight;


    public MainWindow()
    {
        InitializeComponent(attachDevTools: true);

        DataContext = new MainWindowViewModel();

        _dragRegionHeight = TopBar.Height;
        MainBorder.PointerPressed += MainBorder_PointerPressed;

        // Подписываемся на событие изменения состояния окна
        this.GetObservable(WindowStateProperty).Subscribe(state => { MainBorder.Margin = GetMargin(WindowState); });
    }

    /// <summary>
    ///     Функция, возвращающая отступ в зависимости от состояния окна.
    ///     При нормальном состоянии отступ нулевой, при открытии на весь экран появляется отступ для избежания артефактов
    /// </summary>
    /// <param name="windowState">Текущее состояние по которому определяется отступ</param>
    /// <returns></returns>
    private static Thickness GetMargin(WindowState windowState)
    {
        return windowState == WindowState.Maximized ? new Thickness(7) : new Thickness(0);
    }


    private void WindowMinimizeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void WindowCloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Exit(0);
    }

    private void WindowMaximizeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        MainBorder.Margin = GetMargin(WindowState);
    }

    private void MainBorder_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        //Обработка события нажатия два раза на панель управления окном
        if (e.ClickCount == 2)
        {
            WindowState = WindowState.Maximized;
            MainBorder.Margin = GetMargin(WindowState);
        }
        //Обработка перемещения с помощью зажатия на панели управлением окном
        else
        {
            // Проверяем, что событие вызвано левой кнопкой мыши
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

            // Получаем координаты точки нажатия относительно окна
            var position = e.GetPosition(MainBorder);

            // Проверяем, что точка находится в верхней части области MainBorder
            if (!(position.Y <= _dragRegionHeight)) return;

            // Если окно в полноэкранном режиме, возвращаем его в нормальный режим перед началом перетаскивания
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MainBorder.Margin = GetMargin(WindowState);

                // Пересчитываем координаты точки нажатия относительно нового размера окна
                var screenPosition = e.GetPosition(this);
                var newLeft = screenPosition.X - Bounds.Width / 2;
                var newTop = screenPosition.Y - _dragRegionHeight;

                Position = new PixelPoint((int)newLeft, (int)newTop);
            }

            // Инициируем перетаскивание окна
            BeginMoveDrag(e);
        }
    }
}