using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using RFD.Core;
using RFD.Interfaces;
using RFD.UserControls;
using RFD.ViewModels;

namespace RFD.Views;

public partial class MainWindow : Window
{
    /// <summary>
    ///     Поле получающее значение высоты верхней панели управления приложением
    /// </summary>
    private readonly double _dragRegionHeight;
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Вставим платформо-зависимую настройку для маков
        if (PlatformUtils.IsMacOS)
        {
            //this.Icon = new WindowIcon("Assets/app-icon.icns");
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.PreferSystemChrome;
            ExtendClientAreaTitleBarHeightHint = -1;
            
            TitleApp.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            ButtonsApp.IsVisible = false;
        }
        else
        {
            this.Icon = new WindowIcon("Assets/app-icon.ico");
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
        }

        _dragRegionHeight = TopBar.Height;
        MainBorder.PointerPressed += MainBorder_PointerPressed;

        this.GetObservable(WindowStateProperty).Subscribe(state => MainBorder.Margin = GetMargin(this.WindowState));
        
        LeftGrid.GetObservable(IsVisibleProperty).Subscribe(state => { Console.WriteLine($"Отображение левой панели: {state}"); });
        RightGrid.GetObservable(IsVisibleProperty).Subscribe(state => { Console.WriteLine($"Отображение правой панели: {state}"); });
    }
    
    /// <summary>
    ///     Функция, возвращающая отступ в зависимости от состояния окна.
    ///     При нормальном состоянии отступ нулевой, при открытии на весь экран появляется отступ для избежания артефактов
    /// </summary>
    /// <param name="windowState">Текущее состояние по которому определяется отступ</param>
    /// <returns></returns>
    private static Thickness GetMargin(WindowState windowState)
    {
        return PlatformUtils.IsMacOS ? new Thickness(0)
            : windowState == WindowState.Maximized ? new Thickness(7)
            : new Thickness(0);
    }


    private void WindowMinimizeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void WindowCloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        //TODO Реализовать нормальное выключение приложения
        Environment.Exit(0);
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