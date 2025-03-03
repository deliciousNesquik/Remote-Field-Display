using System;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using ReactiveUI;
using RFD.Interfaces;

namespace RFD.ViewModels;

public class ParametersSectionViewModel : INotifyPropertyChanged
{
    
    private readonly IWindowService _windowService = null!;

    public double SelectedWidth { get; set; } = 800;

    public double SelectedHeight { get; set; } = 500;
    
    private double _magneticDeclination;

    public double MagneticDeclination
    {
        get => _magneticDeclination;
        set
        {
            _magneticDeclination = value;
            OnPropertyChanged();
        }
    }
    private double _toolfaceOffset;
    public double ToolfaceOffset {
        get => _toolfaceOffset;
        set
        {
            _toolfaceOffset = value;
            OnPropertyChanged();
        }
    }
    private TimeSpan _timeStamp;
    public TimeSpan TimeStamp {
        get => _timeStamp;
        set
        {
            _timeStamp = value;
            OnPropertyChanged();
        }
    }
    
    private double _angle;
    public double Angle {
        get => _angle;
        set
        {
            _angle = value;
            OnPropertyChanged();
        }
    }
    
    private string _toolfacetype = "";
    public string ToolfaceType {
        get => _toolfacetype;
        set
        {
            _toolfacetype = value;
            OnPropertyChanged();
        }
    }

    public ReactiveCommand<Unit, Unit> OpenInNewWindowCommand { get; }
    public ParametersSectionViewModel(IWindowService windowService)
    {
        MagneticDeclination = 0.0;
        ToolfaceOffset = 0.0;
        TimeStamp = TimeSpan.Zero;
        Angle = 0.0;
        ToolfaceType = "Нет данных";
        
        _windowService = windowService;
        OpenInNewWindowCommand = ReactiveCommand.Create(OpenInNewWindow);
    }
    
    public void SetTime(NPFGEO.LWD.Net.DataObject data)
    {
        TimeStamp = data.TimeStamp == TimeSpan.MaxValue ? TimeSpan.FromSeconds(0) : data.TimeStamp;
    }

    private void OpenInNewWindow()
    {
        var newControl = new UserControls.ParametersSection() { DataContext = this };
        _windowService.OpenWindow(newControl, "Параметры");
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}