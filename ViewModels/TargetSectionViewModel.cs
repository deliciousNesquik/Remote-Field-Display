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

public partial class TargetSectionViewModel : INotifyPropertyChanged
{
    public ReactiveCommand<Unit, Unit> OpenInNewWindowCommand { get; }

    private List<Point> _sector;

    public List<Point> Sector
    {
        get => _sector;
        set
        {
            _sector = value;
            OnPropertyChanged();
        }
    }

    public TargetSectionViewModel()
    {
        OpenInNewWindowCommand = ReactiveCommand.Create(OpenInNewWindow);
    }

    /// <summary>
    /// Выделяет область в мишени начиная со стартового угла заканчивая конечным
    /// </summary>
    /// <param name="startAngle">Начальный угол сектора (0 - 180; -179 - -1)</param>
    /// <param name="endAngle">Конечный угол сектора (0 - 180; -179 - -1)</param>
    public void SetSector(double startAngle, double endAngle)
    {
        Sector = CreateSectorPoints(new Point(100, 100), GetPointForAngle(startAngle), GetPointForAngle(endAngle), 100);
    }

    private void OpenInNewWindow()
    {
        var window = new Window
        {
            Title = "Мишень",
            Content = new UserControls.TargetSection { DataContext = this },
            Width = 500,
            Height = 400
        };
        window.Show();
    }
    private static Point GetPointForAngle(double angleDegrees)
    {
        // Исходные данные: для 0° точка (100,10)
        // Центр определяется как (100, r + 10), а r вычисляется из условия для 45°.
        double cx = 100;
    
        // Для 45°: x = 163 = 100 + r * sin(45°)
        // r = (163 - 100) / sin(45°)
        double r = (163 - 100) / Math.Sin(Math.PI / 4);  // sin(45°) = √2/2
    
        double cy = r + 10;
    
        // Переводим угол в радианы
        double rad = angleDegrees * Math.PI / 180.0;
    
        double x = cx + r * Math.Sin(rad);
        double y = cy - r * Math.Cos(rad);
    
        return new Point(x, y);
    }
    private static List<Point> CreateSectorPoints(Point center, Point start, Point end, int segments)
    {
        var points = new List<Point>();
        // Добавляем центр сектора (если нужен, например, для построения сектора как многоугольника)
        points.Add(center);

        // Определяем угол для начальной и конечной точек (в радианах)
        double angleStart = Math.Atan2(start.Y - center.Y, start.X - center.X);
        double angleEnd = Math.Atan2(end.Y - center.Y, end.X - center.X);

        // Если разница углов отрицательная, можно скорректировать (в зависимости от того, в какую сторону строим дугу)
        if (angleEnd < angleStart)
        {
            angleEnd += 2 * Math.PI;
        }

        // Добавляем первую точку дуги (начальную)
        points.Add(start);

        // Вычисляем длину шага по углу между точками
        double step = (angleEnd - angleStart) / segments;

        // В цикле генерируем промежуточные точки
        // Начинаем с 1, чтобы не добавить повторно начальную точку
        for (int i = 1; i < segments; i++)
        {
            double angle = angleStart + step * i;
            // Предполагаем, что все точки лежат на окружности с радиусом, равным расстоянию от центра до начальной точки.
            double radius = Math.Sqrt(Math.Pow(start.X - center.X, 2) + Math.Pow(start.Y - center.Y, 2));
            var pt = new Point(
                center.X + radius * Math.Cos(angle),
                center.Y + radius * Math.Sin(angle)
            );
            points.Add(pt);
        }

        // Добавляем конечную точку дуги
        points.Add(end);

        return points;
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}