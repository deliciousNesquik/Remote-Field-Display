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

    public TargetSectionViewModel()
    {
        OpenInNewWindowCommand = ReactiveCommand.Create(OpenInNewWindow);
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public StreamGeometry CreateSectorGeometry(double startAngle, double endAngle, double innerRadius, double outerRadius)
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            var center = new Point(100, 100);
            var startOuter = GetPointOnCircle(center, outerRadius, startAngle);
            var endOuter = GetPointOnCircle(center, outerRadius, endAngle);
            var startInner = GetPointOnCircle(center, innerRadius, endAngle);
            var endInner = GetPointOnCircle(center, innerRadius, startAngle);

            ctx.BeginFigure(startOuter, true);
            ctx.ArcTo(endOuter, new Size(outerRadius, outerRadius), 0, endAngle - startAngle > 180, SweepDirection.Clockwise);
            ctx.LineTo(startInner);
            ctx.ArcTo(endInner, new Size(innerRadius, innerRadius), 0, endAngle - startAngle > 180, SweepDirection.Clockwise);
        }
        return geometry;
    }

    private Point GetPointOnCircle(Point center, double radius, double angle)
    {
        double radians = Math.PI * angle / 180.0;
        return new Point(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
    }
}