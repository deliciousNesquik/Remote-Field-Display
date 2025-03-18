using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia;
using Avalonia.Media;
using ReactiveUI;
using RFD.Interfaces;
using RFD.Models;
using Point = Avalonia.Point;

namespace RFD.ViewModels;

public class TargetSectionViewModel : ViewModelBase
{
    private readonly object _updateLock = new(); // Объект для блокировки
    private readonly IWindowService _windowService;
    public ReactiveCommand<Unit, Unit> OpenInNewWindowCommand { get; }

    public Point Center { get; set; }

    #region Настройки сетки мишени (Вместимость ; Угл.сетка ; Ширина ; Шрифт ; (-180 -> 180))

    public ObservableCollection<Ring> DrillingRingsList { get; set; }
    public ObservableCollection<GridLine> RadialLinesList { get; set; }

    private Point _startPointVertical;
    public Point StartPointVertical
    {
        get => _startPointVertical;
        set => this.RaiseAndSetIfChanged(ref _startPointVertical, value);
    }

    private Point _endPointVertical;
    public Point EndPointVertical
    {
        get => _endPointVertical;
        set => this.RaiseAndSetIfChanged(ref _endPointVertical, value);
    }

    private Point _startPointHorizontal;
    public Point StartPointHorizontal
    {
        get => _startPointHorizontal;
        set => this.RaiseAndSetIfChanged(ref _startPointHorizontal, value);
    }

    private Point _endPointHorizontal;
    public Point EndPointHorizontal
    {
        get => _endPointHorizontal;
        set => this.RaiseAndSetIfChanged(ref _endPointHorizontal, value);
    }

    private double _strokeThickness = 0.5;
    public double StrokeThickness
    {
        get => _strokeThickness;
        set
        {
            this.RaiseAndSetIfChanged(ref _strokeThickness, value);
            UpdateTarget();
        }
    }

    private int _capacity;
    public int Capacity
    {
        get => _capacity;
        set
        {
            this.RaiseAndSetIfChanged(ref _capacity, value);
            UpdateTarget();
        }
    }

    private bool _isHalfMode;
    public bool IsHalfMode
    {
        get => _isHalfMode;
        set => this.RaiseAndSetIfChanged(ref _isHalfMode, value);
    }

    private int _gridFrequency = 45;
    public int GridFrequency
    {
        get => _gridFrequency;
        set
        {
            this.RaiseAndSetIfChanged(ref _gridFrequency, value);
            UpdateTarget();
        }
    }

    private int _fontSize;
    public int FontSize
    {
        get => _fontSize;
        set
        {
            this.RaiseAndSetIfChanged(ref _fontSize, value);
            UpdateTarget();
        }
    }

    private double _ringWidth;
    public double RingWidth
    {
        get => _ringWidth;
        set
        {
            this.RaiseAndSetIfChanged(ref _ringWidth, value);
            UpdateTarget();
        }
    }

    private double _ringThickness;
    public double RingThickness
    {
        get => _ringThickness;
        set
        {
            this.RaiseAndSetIfChanged(ref _ringThickness, value);
            UpdateTarget();
        }
    }

    public ObservableCollection<AnglePoint> AngleLabelsList { get; set; }
    #endregion

    #region Настройки точек мишени (Радиус точки ; Коэффицент уменьшения радиуса в зависимости от удаленности от края ; От центра к краю)

    private double _defaultRadius;
    public double DefaultRadius
    {
        get => _defaultRadius;
        set
        {
            this.RaiseAndSetIfChanged(ref _defaultRadius, value);
            UpdateTarget();
        }
    }

    public double ReductionFactor { get; set; }
    public bool FromCenterToBorder { get; set; }

    public ObservableCollection<DrillingPoints> DrillingPointsList { get; set; }
    #endregion

    #region Настройки сектора мишени (Путь ; Сглаженность ; Цвет)
    private List<Point> _sector = new List<Point>([new Point(100, 100)]);
    public List<Point> Sector
    {
        get => _sector;
        set => this.RaiseAndSetIfChanged(ref _sector, value);
    }

    private IBrush _sectorColor = Brush.Parse("#2B0068FF");
    public IBrush SectorColor
    {
        get => _sectorColor;
        set => this.RaiseAndSetIfChanged(ref _sectorColor, value);
    }

    private const int SectorSmooth = 100;
    #endregion

    private void UpdateTarget()
    {
        lock (_updateLock) // Блокируем обновление
        {
            Center = new Point(100 + (RingThickness - 10), 100 + (RingThickness - 10));

            StartPointVertical = new Point(100 + (RingThickness - 10), 10 + (RingThickness - 10));
            EndPointVertical = new Point(100 + (RingThickness - 10), 190 + (RingThickness - 10));

            StartPointHorizontal = new Point(10 + (RingThickness - 10), 100 + (RingThickness - 10));
            EndPointHorizontal = new Point(190 + (RingThickness - 10), 100 + (RingThickness - 10));

            UpdateRadialLines();
            UpdateRings();
            UpdateAngleLabels();
            UpdatePoints();
        }
    }

    private void UpdateRadialLines()
    {
        RadialLinesList.Clear();
        for (var angle = 0; angle <= 360; angle += GridFrequency)
        {
            if (angle is 0 or 90 or 180 or 270 or 360) continue;
            RadialLinesList.Add(new GridLine(angle, GetPointForAngle(angle, Center, 90), Center));
        }
    }

    private void UpdateRings()
    {
        DrillingRingsList.Clear();
        var distance = 180.0 / (Capacity - 1);
        for (var i = 1; i <= Capacity - 1; i++)
        {
            DrillingRingsList.Add(new Ring(i * distance, i * distance, i, (i * distance) / 2, new Thickness(RingThickness)));
        }
    }

    private void UpdateAngleLabels()
    {
        AngleLabelsList.Clear();
        for (var angle = 0; angle < 360; angle += GridFrequency)
        {
            var width = ($"{angle}°".Length - 0.5) * 0.6 * FontSize;
            var height = 1.2 * FontSize;

            var leftMargin = GetMidPoint(GetPointForAngle(angle, Center, 90), RingThickness, angle).X - (width / 2);
            var topMargin = GetMidPoint(GetPointForAngle(angle, Center, 90), RingThickness, angle).Y - (height / 2);

            AngleLabelsList.Add(new AnglePoint($"{angle}°", new Thickness(leftMargin, topMargin, 0, 0), FontSize));
        }
    }

    private void UpdatePoints()
    {
        while (DrillingPointsList.Count < Capacity - 1)
        {
            DrillingPointsList.Add(new DrillingPoints(new Thickness(0, 0, 0, 0), DefaultRadius));
        }

        while (DrillingPointsList.Count > Capacity - 1)
        {
            DrillingPointsList.RemoveAt(DrillingPointsList.Count - 1);
        }

        foreach (var point in DrillingPointsList)
        {
            point.Size = DefaultRadius / 2;
        }
    }

    public TargetSectionViewModel(IWindowService windowService)
    {
        Center = new Point(100, 100);
        DrillingRingsList = new ObservableCollection<Ring>();
        RadialLinesList = new ObservableCollection<GridLine>();
        AngleLabelsList = new ObservableCollection<AnglePoint>();
        DrillingPointsList = new ObservableCollection<DrillingPoints>();

        Capacity = 6;
        GridFrequency = 45;
        IsHalfMode = false;
        RingThickness = RingWidth = 10;
        FontSize = 10;

        UpdateTarget();

        _windowService = windowService;
        OpenInNewWindowCommand = ReactiveCommand.Create(OpenInNewWindow);
    }

    public void SetSector(double startAngle, double endAngle)
    {
        Sector = CreateSectorPoints(
            Center,
            GetPointForAngle(startAngle, Center, 90),
            GetPointForAngle(endAngle, Center, 90),
            SectorSmooth
        );
    }

    public void ClearSector() => Sector = [Center];
    public void SetSectorColor(IBrush color) => SectorColor = color;

    public void SetPoint(int index, double angle)
    {
        if (index < 0 || index >= DrillingPointsList.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        var radius = (index + 1) * (180.0 / (Capacity - 1));
        var newPoint = GetPointForAngle(angle, new Point(0, 0), radius);

        DrillingPointsList[index].Margin = new Thickness(newPoint.X, newPoint.Y, 0, 0);
        Console.WriteLine($"point [{index}] - radius - [{radius}] - margin - [{DrillingPointsList[index].Margin}]");
    }

    private void OpenInNewWindow()
    {
        var newControl = new UserControls.TargetSection() { DataContext = this };
        _windowService.OpenWindow(newControl, "Мишень");
    }

    private static Point GetMidPoint(Point innerPoint, double distance, double angleDegrees)
    {
        var angleRadians = angleDegrees * Math.PI / 180.0;
        var directionX = Math.Sin(angleRadians);
        var directionY = -Math.Cos(angleRadians);
        var offset = distance / 2.0;
        var newX = innerPoint.X + (directionX * offset);
        var newY = innerPoint.Y + (directionY * offset);
        return new Point(newX, newY);
    }

    private static Point GetPointForAngle(double angle, Point center, double radius)
    {
        var rad = Math.PI * angle / 180.0;
        return new Point(center.X + radius * Math.Sin(rad), center.Y - radius * Math.Cos(rad));
    }

    private static List<Point> CreateSectorPoints(Point center, Point start, Point end, int segments)
    {
        List<Point> points = [center, start];
        var startAngle = Math.Atan2(start.Y - center.Y, start.X - center.X);
        var endAngle = Math.Atan2(end.Y - center.Y, end.X - center.X);
        if (endAngle < startAngle) endAngle += 2 * Math.PI;
        var step = (endAngle - startAngle) / segments;

        for (var i = 1; i < segments; i++)
        {
            var angle = startAngle + step * i;
            var radius = Math.Sqrt(Math.Pow(start.X - center.X, 2) + Math.Pow(start.Y - center.Y, 2));
            points.Add(new Point(center.X + radius * Math.Cos(angle), center.Y + radius * Math.Sin(angle)));
        }
        points.Add(end);
        return points;
    }
}