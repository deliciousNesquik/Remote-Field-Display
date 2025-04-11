using Avalonia;
using ReactiveUI;
using RFD.Interfaces;

namespace RFD.Models;

public class Ring : ViewModelBase
{
    private double _height;
    private Thickness _margin;
    private int _order;
    private double _radius;
    private double _width;

    public Ring(double width, double height, int order, double radius, Thickness margin)
    {
        _width = width;
        _height = height;
        _order = order;
        _radius = radius;
        _margin = margin;
    }

    public double Width
    {
        get => _width;
        set => this.RaiseAndSetIfChanged(ref _width, value);
    }

    public double Height
    {
        get => _height;
        set => this.RaiseAndSetIfChanged(ref _height, value);
    }

    public int Order
    {
        get => _order;
        set => this.RaiseAndSetIfChanged(ref _order, value);
    }

    public double Radius
    {
        get => _radius;
        set => this.RaiseAndSetIfChanged(ref _radius, value);
    }

    public Thickness Margin
    {
        get => _margin;
        set => this.RaiseAndSetIfChanged(ref _margin, value);
    }
}