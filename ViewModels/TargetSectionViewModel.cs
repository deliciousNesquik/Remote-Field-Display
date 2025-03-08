using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using RFD.Interfaces;
using RFD.Models;

namespace RFD.ViewModels;

public class TargetSectionViewModel: INotifyPropertyChanged
{
    /// <summary>Сервис для создания окна для данного элемента</summary>
    private readonly IWindowService _windowService;
    public ReactiveCommand<Unit, Unit> OpenInNewWindowCommand { get; }
    
    public Point Center { get; set; }
    
    #region Настройки сетки мишени (Вместимость ; Угл.сетка ; Ширина ; Шрифт ; (-180 -> 180))

    public ObservableCollection<Ring> InnerLine { get; set; }
    public ObservableCollection<GridLine> GridLine { get; set; }
    
    private Point _startPointVertical;
    public Point StartPointVertical
    {
        get => _startPointVertical;
        set
        {
            _startPointVertical = value;
            OnPropertyChanged();
        }
    }
    private Point _endPointVertical;
    public Point EndPointVertical
    {
        get => _endPointVertical;
        set
        {
            _endPointVertical = value;
            OnPropertyChanged();
        }
    }
    private Point _startPointHorizontal;
    public Point StartPointHorizontal
    {
        get => _startPointHorizontal;
        set
        {
            _startPointHorizontal = value;
            OnPropertyChanged();
        }
    }
    private Point _endPointHorizontal;
    public Point EndPointHorizontal
    {
        get => _endPointHorizontal;
        set
        {
            _endPointHorizontal = value;
            OnPropertyChanged();
        }
    }
    
    private double _strokeThickness = 0.5;
    public double StrokeThickness
    {
        get => _strokeThickness;
        set
        {
            _strokeThickness = value;
            UpdateTarget();
            
            OnPropertyChanged();
        }
    }
    
    private int _capacity;
    public int Capacity
    {
        get => _capacity;
        set
        {
            //При изменении вместимости, изменяется количество точек отображаемых на мишени
            _points = new Thickness[_capacity];
            
            //При передаче вместимости, Genesis LWD считает что внутренняя точка является тоже окружностью 
            _capacity = value;
            UpdateTarget();

            OnPropertyChanged();
        }
    }

    public bool IsHalfMode { get; set; }
    private int _gridFrequency = 45;
    public int GridFrequency
    {
        get => _gridFrequency;
        set
        {
            _gridFrequency = value;
            OnPropertyChanged();
            UpdateTarget();
        }
    }
    private int _fontSize;

    public int FontSize
    {
        get => _fontSize;
        set
        {
            _fontSize = value / 2;
            UpdateTarget();
            OnPropertyChanged();
        }
    }
    private double _ringWidth;
    public double RingWidth
    {
        get => _ringWidth;
        set
        {
            _ringWidth = 2.0 * value + 180;
            UpdateTarget();
            OnPropertyChanged();
        }
    }
    private double _ringThickness;
    public double RingThickness
    {
        get => _ringThickness;
        set
        {
            _ringThickness = value;
            UpdateTarget();
            OnPropertyChanged();
        }
    }
    

    #endregion

    #region Настройки точек мишени (Радиус точки ; Коэффицент уменьшения радиуса в зависимости от удаленности от края ; От центра к краю)
    public double DefaultRadius { get; set; }
    public double ReductionFactor { get; set; }
    public bool FromCenterToBorder { get; set; }
    
    private Thickness[] _points = new Thickness[5];
    #endregion

    #region Настройки сектора мишени (Путь ; Сглаженность ; Цвет)
    private List<Point> _sector = new List<Point>([new Point(100, 100)]);
    public List<Point> Sector
    {
        get => _sector;
        set
        {
            _sector = value;
            OnPropertyChanged();
        }
    }
    private IBrush _sectorColor = Brush.Parse("#2B0068FF");
    public IBrush SectorColor
    {
        get => _sectorColor;
        set => _sectorColor = value;
    }
    
    private const int SectorSmooth = 100;
    #endregion
    

    private void UpdateTarget()
    {
        Center = new Point(100 + (RingThickness - 10), 100 + (RingThickness - 10));
        
        StartPointVertical = new Point(100 + (RingThickness - 10), 10 + (RingThickness - 10));
        EndPointVertical = new Point(100 + (RingThickness - 10), 190 + (RingThickness - 10));
        
        StartPointHorizontal = new Point(10 + (RingThickness - 10), 100 + (RingThickness - 10));
        EndPointHorizontal = new Point(190 + (RingThickness - 10), 100 + (RingThickness - 10));
        
        GridLine.Clear();
        for (int angle = 0; angle <= 360; angle += GridFrequency)
        {
            if (angle is 0 or 90 or 180 or 270 or 360)
            {
                continue;
            }
            Console.WriteLine($"Angle: ({angle}) ; PointOfAngle: ({GetPointForAngle(angle, Center, 90)}) ; CenterPoint: ({Center})");
            GridLine.Add(new GridLine(angle, GetPointForAngle(angle, Center, 90), Center));
        }

        //Console.WriteLine($"start point vertical line - ({StartPointVertical.X} | {StartPointVertical.Y})");
        //Console.WriteLine($"start point horizontal line - ({StartPointHorizontal.X} | {StartPointHorizontal.Y})");
        
        InnerLine.Clear();
        var distance = 180.0 / (Capacity - 1);
        Console.WriteLine($"Capacity: {Capacity}");
        Console.WriteLine($"Distance inner line: {distance}");

        for (int i = 1; i <= Capacity - 1; i++) {
            InnerLine.Add(new Ring(i * distance, i * distance, i, (i * distance) / 2, new Thickness(RingThickness)));
        }


        foreach (var ringsTarget in InnerLine)
        {
            Console.WriteLine($"index ring:{ringsTarget.Order} - (radius {ringsTarget.Radius}) - (width:{ringsTarget.Width}; height:{ringsTarget.Height})");
        }
    }
    

    

    
    public Thickness Point1 => _points[0];
    public Thickness Point2 => _points[1];
    public Thickness Point3 => _points[2];
    public Thickness Point4 => _points[3];
    public Thickness Point5 => _points[4];
    
    public TargetSectionViewModel(IWindowService windowService)
    {
        Center = new Point(100, 100);
        InnerLine = new ObservableCollection<Ring>();
        GridLine = new ObservableCollection<GridLine>();
        Capacity = 6;
        GridFrequency = 45;
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
        if (index < 0 || index >= _points.Length) return;
        double radius = (index + 1) * 36;
        var newPoint = GetPointForAngle(angle, new Point(0, 0), radius);
        _points[index] = new Thickness(newPoint.X, newPoint.Y, 0, 0);
        OnPropertyChanged($"Point{index + 1}");
    }
    
    private void OpenInNewWindow()
    {
        var newControl = new UserControls.TargetSection() { DataContext = this };
        _windowService.OpenWindow(newControl, "Мишень");
    }

    private static Point GetPointForAngle(double angle, Point center, double radius)
    {
        double rad = Math.PI * angle / 180.0;
        return new Point(center.X + radius * Math.Sin(rad), center.Y - radius * Math.Cos(rad));
    }
    
    private static List<Point> CreateSectorPoints(Point center, Point start, Point end, int segments)
    {
        List<Point> points = [center, start];
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
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}