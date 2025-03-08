using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using ReactiveUI;
using RFD.Interfaces;

namespace RFD.Models;

public class GridLine: ViewModelBase
{
    private string _angle;
    private Point _pointOfAngle;
    private Point _pointCenter;

    public string Angle {
        get => _angle;
        set => this.RaiseAndSetIfChanged(ref _angle, value);
    }


    public Point PointOfAngle {
        get => _pointOfAngle;
        set => this.RaiseAndSetIfChanged(ref _pointOfAngle, value);
    }


    public Point PointCenter {
        get => _pointCenter;
        set => this.RaiseAndSetIfChanged(ref _pointCenter, value);

    }

    public GridLine(int angle, Point pointOfAngle, Point pointCenter)
    {
        _angle = $"{angle}°";
        _pointOfAngle = pointOfAngle;
        _pointCenter = pointCenter;
    }
}