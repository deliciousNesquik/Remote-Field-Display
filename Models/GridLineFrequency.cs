using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;

namespace RFD.Models;

public class GridLineFrequency: INotifyPropertyChanged
{
    private string _angle = "°";
    public string Angle
    {
        get => _angle;
        set
        {
            _angle = value;
            OnPropertyChanged();
        }
    }

    private Point _pointOfAngle;

    public Point PointOfAngle
    {
        get => _pointOfAngle;
        set
        {
            _pointOfAngle = value;
            OnPropertyChanged();
        }
    }

    private Point _pointCenter;

    public Point PointCenter
    {
        get => _pointCenter;
        set
        {
            _pointCenter = value;
            OnPropertyChanged();
        }
    }

    public GridLineFrequency(int angle, Point pointOfAngle, Point pointCenter)
    {
        Angle = $"{angle}°";
        PointOfAngle = pointOfAngle;
        PointCenter = pointCenter;
    }
    
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}