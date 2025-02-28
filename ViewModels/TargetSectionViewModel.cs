using System;
using System.Collections.Generic;
using System.Reactive;
using Avalonia;
using Avalonia.Media;
using ReactiveUI;
using RFD.Interfaces;

namespace RFD.ViewModels;

/// <summary>
/// TargetSectionViewModel — это модель представления для отображения мишени с круговой сеткой,
/// на которой отображается сектор бурения. Модель позволяет настраивать сектор
/// (с заданием углов и ширины) и отображать точки бурения в пределах мишени.
/// Предоставляет удобные методы для настройки и отображения мишени с круговой сеткой,
/// отображающей сектор бурения и точки. Модель легко интегрируется с интерфейсом и
/// позволяет динамически изменять параметры, такие как углы, цвета и расположение точек бурения.
/// </summary>
public class TargetSectionViewModel : ReactiveObject
{
    #region Открытие UserControl в отдельном окне
    /// <summary>
    /// Сервис для создания данного UserControl в отдельном окне
    /// </summary>
    private readonly IWindowService _windowService;

    /// <summary>
    /// Команда для открытия мишени в новом окне
    /// </summary>
    public ReactiveCommand<Unit, Unit> OpenInNewWindowCommand { get; }

    /// <summary>
    /// Открывает мишень в отдельном окне
    /// </summary>
    private void OpenInNewWindow()
    {
        _windowService.OpenWindow(
            new UserControls.TargetSection { DataContext = this }, 
            "Мишень");
    }
    #endregion

    #region Параметры сектора мишени
    private List<Point> _sector = new List<Point> { CenterTarget };

    /// <summary>
    /// Список точек, описывающих сектор на мишени
    /// </summary>
    public List<Point> Sector
    {
        get => _sector;
        set => this.RaiseAndSetIfChanged(ref _sector, value);
    }

    private IBrush _sectorColor = Brush.Parse("#2B0068FF");

    /// <summary>
    /// Цвет сектора
    /// </summary>
    public IBrush SectorColor
    {
        get => _sectorColor;
        set => this.RaiseAndSetIfChanged(ref _sectorColor, value);
    }

    private const int SmoothingCorners = 100;

    /// <summary>
    /// Устанавливает сектор мишени по заданным углам
    /// </summary>
    public void SetSector(double startAngle, double endAngle)
    {
        Sector = CreateSectorPoints(
            CenterTarget,
            GetPointForAngle(startAngle, CenterTarget, 90),
            GetPointForAngle(endAngle, CenterTarget, 90),
            SmoothingCorners
        );
    }

    /// <summary>
    /// Устанавливает цвет сектора
    /// </summary>
    public void SetSectorColor(IBrush color) => SectorColor = color;

    /// <summary>
    /// Очищает сектор, устанавливая только центральную точку
    /// </summary>
    public void ClearSector() => Sector = new List<Point> { CenterTarget };
    #endregion

    #region Параметры точек мишени
    private readonly List<Thickness> _points = new List<Thickness>(5);

    /// <summary>
    /// Точка 1 на мишени
    /// </summary>
    public Thickness Point1 => _points.Count > 0 ? _points[0] : new Thickness(0);

    /// <summary>
    /// Точка 2 на мишени
    /// </summary>
    public Thickness Point2 => _points.Count > 1 ? _points[1] : new Thickness(0);

    /// <summary>
    /// Точка 3 на мишени
    /// </summary>
    public Thickness Point3 => _points.Count > 2 ? _points[2] : new Thickness(0);

    /// <summary>
    /// Точка 4 на мишени
    /// </summary>
    public Thickness Point4 => _points.Count > 3 ? _points[3] : new Thickness(0);

    /// <summary>
    /// Точка 5 на мишени
    /// </summary>
    public Thickness Point5 => _points.Count > 4 ? _points[4] : new Thickness(0);

    /// <summary>
    /// Устанавливает точку на мишени по индексу и углу
    /// </summary>
    public void SetPoint(int index, double angle)
    {
        if (index < 0 || index >= _points.Count) return;
        double radius = (index + 1) * 36;
        var newPoint = GetPointForAngle(angle, new Point(0, 0), radius);
        _points[index] = new Thickness(newPoint.X, newPoint.Y, 0, 0);
        this.RaisePropertyChanged($"Point{index + 1}");
    }
    #endregion
    
    private static readonly Point CenterTarget = new(100, 100);

    /// <summary>
    /// Конструктор модели
    /// </summary>
    public TargetSectionViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        OpenInNewWindowCommand = ReactiveCommand.Create(OpenInNewWindow);
    }

    /// <summary>
    /// Вычисляет точку для заданного угла на окружности с учетом центра и радиуса
    /// </summary>
    private static Point GetPointForAngle(double angle, Point center, double radius)
    {
        double rad = Math.PI * angle / 180.0;
        return new Point(center.X + radius * Math.Sin(rad), center.Y - radius * Math.Cos(rad));
    }

    /// <summary>
    /// Создает список точек для сектора мишени на основе начальной и конечной точки и заданного числа сегментов
    /// </summary>
    private static List<Point> CreateSectorPoints(Point center, Point start, Point end, int segments)
    {
        List<Point> points = new List<Point> { center, start };
        double startAngle = Math.Atan2(start.Y - center.Y, start.X - center.X);
        double endAngle = Math.Atan2(end.Y - center.Y, end.X - center.X);
        
        if (endAngle < startAngle) endAngle += 2 * Math.PI;
        
        double step = (endAngle - startAngle) / segments;

        for (int i = 1; i < segments; i++)
        {
            double angle = startAngle + step * i;
            double radius = Math.Sqrt(Math.Pow(start.X - center.X, 2) + Math.Pow(start.Y - center.Y, 2));
            points.Add(new Point(center.X + radius * Math.Cos(angle), center.Y + radius * Math.Sin(angle)));
        }
        
        points.Add(end);
        return points;
    }
}
