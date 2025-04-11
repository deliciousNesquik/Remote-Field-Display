using Avalonia;
using ReactiveUI;
using RFD.Interfaces;

namespace RFD.Models;

public class AnglePoint : ViewModelBase
{
    private string _angle;
    private int _fontSize;
    private Thickness _pointAngle;

    public AnglePoint(string angle, Thickness pointAngle, int fontSize)
    {
        _angle = angle;
        _pointAngle = pointAngle;
        _fontSize = fontSize;
    }

    public string Angle
    {
        get => _angle;
        set => this.RaiseAndSetIfChanged(ref _angle, value);
    }

    public Thickness PointAngle
    {
        get => _pointAngle;
        set => this.RaiseAndSetIfChanged(ref _pointAngle, value);
    }

    public int FontSize
    {
        get => _fontSize;
        set => this.RaiseAndSetIfChanged(ref _fontSize, value);
    }
}