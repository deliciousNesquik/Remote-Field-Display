using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using ReactiveUI;

namespace RFD.ViewModels;

public class ParametersSectionViewModel : INotifyPropertyChanged
{
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

    public ReactiveCommand<Unit, Unit> OpenInNewWindowCommand { get; }
    private void OpenInNewWindow()
    {
        var window = new Window
        {
            Content = new UserControls.ParametersSection { DataContext = this },
            Width = SelectedWidth,
            Height = SelectedHeight
        };
        window.Show();
    }
    
    public ParametersSectionViewModel()
    {
        MagneticDeclination = 0.0;
        ToolfaceOffset = 0.0;
        OpenInNewWindowCommand = ReactiveCommand.Create(OpenInNewWindow);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}