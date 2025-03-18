using Avalonia;
using Avalonia.Media;
using ReactiveUI;
using RFD.Interfaces;

namespace RFD.Models;

public class DrillingPoints: ViewModelBase
{
    private Thickness _margin;
    private double _size;
    public Thickness Margin
    {
        get => _margin;
        set => this.RaiseAndSetIfChanged(ref _margin, value);
    }
    public double Size
    {
        get => _size;
        set => this.RaiseAndSetIfChanged(ref _size, value);
    }
    
    /// <inheritdoc />
    public DrillingPoints(Thickness margin, double size)
    {
        _margin = margin;
        _size = size;
    }
}