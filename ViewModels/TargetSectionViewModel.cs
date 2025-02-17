using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;

namespace RFD.ViewModels;

public class TargetSectionViewModel : ReactiveObject
{
    private static readonly Point CenterTarget = new(100, 100);
    private const int SectorSmooth = 100;

    private static int WindowWidth => 500;
    private static int WindowHeight => 400;
    private static string WindowTitle => "Мишень";

    public ReactiveCommand<Unit, Unit> OpenInNewWindowCommand { get; }

    private IBrush _sectorColor = Brush.Parse("#2B0068FF");
    public IBrush SectorColor
    {
        get => _sectorColor;
        set => this.RaiseAndSetIfChanged(ref _sectorColor, value);
    }

    private List<Point> _sector = [CenterTarget];
    public List<Point> Sector
    {
        get => _sector;
        set => this.RaiseAndSetIfChanged(ref _sector, value);
    }

    private readonly Thickness[] _points = new Thickness[4];
    public Thickness Point1 => _points[0];
    public Thickness Point2 => _points[1];
    public Thickness Point3 => _points[2];
    public Thickness Point4 => _points[3];
    
    public TargetSectionViewModel()
    {
        OpenInNewWindowCommand = ReactiveCommand.Create(OpenInNewWindow);
    }
    
    public void SetSector(double startAngle, double endAngle)
    {
        Sector = CreateSectorPoints(
            CenterTarget,
            GetPointForAngle(startAngle, CenterTarget, 90),
            GetPointForAngle(endAngle, CenterTarget, 90),
            SectorSmooth
        );
    }
    public void ClearSector() => Sector = [CenterTarget];
    public void SetSectorColor(IBrush color) => SectorColor = color;
    
    public void SetPoint(int index, double angle)
    {
        if (index < 0 || index >= _points.Length) return;
        double radius = (index + 1) * 36;
        var newPoint = GetPointForAngle(angle, new Point(0, 0), radius);
        _points[index] = new Thickness(newPoint.X, newPoint.Y, 0, 0);
        this.RaisePropertyChanged($"Point{index + 1}");
    }
    
    private void OpenInNewWindow()
    {
        new Window
        {
            Title = WindowTitle,
            Content = new UserControls.TargetSection { DataContext = this },
            Width = WindowWidth,
            Height = WindowHeight
        }.Show();
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
}