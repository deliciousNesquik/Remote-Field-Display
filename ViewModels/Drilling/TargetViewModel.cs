using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media;
using NPFGEO.LWD.Net;
using ReactiveUI;
using RFD.Interfaces;
using RFD.Models;
using RFD.UserControls;
using Point = Avalonia.Point;

namespace RFD.ViewModels;

public class TargetSectionViewModel : INotifyPropertyChanged
{
    private readonly object _updateLock = new(); // Объект для блокировки
    private readonly IWindowService _windowService;
    private double _angle;

    private double _magneticDeclination;
    private TimeSpan _timeStamp;
    private double _toolfaceOffset;
    private string _toolfacetype = "";

    public TargetSectionViewModel(IWindowService windowService)
    {
        Center = new Point(100, 100);
        DrillingRingsList = new ObservableCollection<Ring>();
        RadialLinesList = new ObservableCollection<GridLine>();
        AngleLabelsList = new ObservableCollection<AnglePoint>();
        DrillingPointsList = new ObservableCollection<DrillingPoints> { new(new Thickness(0, 0, 0, 0), DefaultRadius) };

        Capacity = 6;
        GridFrequency = 45;
        IsHalfMode = false;
        RingThickness = RingWidth = 10;
        FontSize = 10;
        ReductionFactor = 1.0; // Значение по умолчанию

        UpdateTarget();

        _windowService = windowService;
        OpenInNewWindowCommand = ReactiveCommand.Create(OpenInNewWindow);
    }

    public ReactiveCommand<Unit, Unit> OpenInNewWindowCommand { get; }

    public Point Center { get; set; }

    public double MagneticDeclination
    {
        get => _magneticDeclination;
        set
        {
            _magneticDeclination = value;
            OnPropertyChanged();
        }
    }

    public double ToolfaceOffset
    {
        get => _toolfaceOffset;
        set
        {
            _toolfaceOffset = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan TimeStamp
    {
        get => _timeStamp;
        set
        {
            _timeStamp = value;
            OnPropertyChanged();
        }
    }

    public double Angle
    {
        get => _angle;
        set
        {
            _angle = value;
            OnPropertyChanged();
        }
    }

    public string ToolfaceType
    {
        get => _toolfacetype;
        set
        {
            _toolfacetype = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetTime(DataObject data)
    {
        TimeStamp = data.TimeStamp == TimeSpan.MaxValue ? TimeSpan.FromSeconds(0) : data.TimeStamp;
    }

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
            DrillingRingsList.Add(new Ring(i * distance, i * distance, i, i * distance / 2,
                new Thickness(RingThickness)));
    }

    private void UpdateAngleLabels()
    {
        AngleLabelsList.Clear();

        if (IsHalfMode)
        {
            // Режим IsHalfMode: от 0° до 180° и от -0° до -180°
            for (var angle = 0; angle <= 180; angle += GridFrequency) AddAngleLabel(angle);
            for (var angle = -GridFrequency; angle > -180; angle -= GridFrequency) AddAngleLabel(angle);
        }
        else
        {
            // Режим !IsHalfMode: от 0° до 360°
            for (var angle = 0; angle < 360; angle += GridFrequency) AddAngleLabel(angle);
        }
    }

    private void AddAngleLabel(double angle)
    {
        var width = ($"{angle}°".Length - 0.5) * 0.6 * FontSize;
        var height = 1.2 * FontSize;

        var leftMargin = GetMidPoint(GetPointForAngle(angle, Center, 90), RingThickness, angle).X - width / 2;
        var topMargin = GetMidPoint(GetPointForAngle(angle, Center, 90), RingThickness, angle).Y - height / 2;

        AngleLabelsList.Add(new AnglePoint($"{angle}°", new Thickness(leftMargin, topMargin, 0, 0), FontSize));
    }

    private void UpdatePoints()
    {
        while (DrillingPointsList.Count < Capacity)
            DrillingPointsList.Add(new DrillingPoints(new Thickness(0, 0, 0, 0), DefaultRadius));

        while (DrillingPointsList.Count > Capacity) DrillingPointsList.RemoveAt(DrillingPointsList.Count - 1);

        // Обновляем размер точек в зависимости от ReductionFactor (начиная с конца списка)
        for (var i = 0; i < DrillingPointsList.Count; i++)
        {
            var reverseIndex = DrillingPointsList.Count - 1 - i; // Индекс с конца
            var reduction = 1 - reverseIndex / (double)(Capacity - 1) * (ReductionFactor - 1);
            DrillingPointsList[i].Size = DefaultRadius / 2 * reduction;
        }
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

    public void ClearSector()
    {
        Sector = [Center];
    }

    public void SetSectorColor(IBrush color)
    {
        SectorColor = color;
    }

    public void SetPoint(int index, double angle)
    {
        if (index < 0 || index >= DrillingPointsList.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");

        var radius = index * (180.0 / (Capacity - 1));
        var newPoint = GetPointForAngle(angle, new Point(0, 0), radius);

        DrillingPointsList[index].Margin = new Thickness(newPoint.X, newPoint.Y, 0, 0);
    }

    private void OpenInNewWindow()
    {
        var newControl = new TargetSection { DataContext = this };
        _windowService.OpenWindow(newControl, "Мишень");
    }

    private static Point GetMidPoint(Point innerPoint, double distance, double angleDegrees)
    {
        var angleRadians = angleDegrees * Math.PI / 180.0;
        var directionX = Math.Sin(angleRadians);
        var directionY = -Math.Cos(angleRadians);
        var offset = distance / 2.0;
        var newX = innerPoint.X + directionX * offset;
        var newY = innerPoint.Y + directionY * offset;
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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region Настройки сетки мишени (Вместимость ; Угл.сетка ; Ширина ; Шрифт ; (-180 -> 180))

    public ObservableCollection<Ring> DrillingRingsList { get; set; }
    public ObservableCollection<GridLine> RadialLinesList { get; set; }

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
            _capacity = value;
            UpdateTarget();
            OnPropertyChanged();
        }
    }

    private bool _isHalfMode;

    public bool IsHalfMode
    {
        get => _isHalfMode;
        set
        {
            _isHalfMode = value;
            UpdateTarget(); // Обновляем мишень при изменении режима
            OnPropertyChanged();
        }
    }

    private int _gridFrequency = 45;

    public int GridFrequency
    {
        get => _gridFrequency;
        set
        {
            _gridFrequency = value;
            UpdateTarget();
            OnPropertyChanged();
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

    public ObservableCollection<AnglePoint> AngleLabelsList { get; set; }

    #endregion

    #region Настройки точек мишени (Радиус точки ; Коэффицент уменьшения радиуса в зависимости от удаленности от края ; От центра к краю)

    private double _defaultRadius;

    public double DefaultRadius
    {
        get => _defaultRadius;
        set
        {
            _defaultRadius = value;
            UpdateTarget();
            OnPropertyChanged();
        }
    }

    private double _reductionFactor = 1.0; // Значение по умолчанию

    public double ReductionFactor
    {
        get => _reductionFactor;
        set
        {
            if (value < 1.0 || value > 1.5)
                throw new ArgumentOutOfRangeException(nameof(value), "ReductionFactor must be between 1.0 and 1.5.");

            _reductionFactor = value;
            UpdateTarget(); // Обновляем мишень при изменении коэффициента
            OnPropertyChanged();
        }
    }

    public bool FromCenterToBorder { get; set; }

    public ObservableCollection<DrillingPoints> DrillingPointsList { get; set; }

    #endregion

    #region Настройки сектора мишени (Путь ; Сглаженность ; Цвет)

    private List<Point> _sector = new([new Point(100, 100)]);

    public List<Point> Sector
    {
        get => _sector;
        set
        {
            _sector = value;
            OnPropertyChanged();
        }
    }

    public IBrush SectorColor { get; set; } = Brush.Parse("#2B0068FF");

    private const int SectorSmooth = 100;

    #endregion
}