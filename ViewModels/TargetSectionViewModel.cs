using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using ReactiveUI;

namespace RFD.ViewModels;

public partial class TargetSectionViewModel : INotifyPropertyChanged
{
    private string _selectedSize = "Fill";

    public string SelectedSize
    {
        get => _selectedSize;
        set
        {
            OnPropertyChanged(SelectedSize);
            _selectedSize = value;
        }
    }

    public double SelectedWidth { get; set; } = 800;

    public double SelectedHeight { get; set; } = 500;

    public ReactiveCommand<Unit, Unit> OpenInNewWindowCommand { get; }

    public TargetSectionViewModel()
    {
        OpenInNewWindowCommand = ReactiveCommand.Create(OpenInNewWindow);
    }

    private void UpdateSize(string size)
    {
        switch (size)
        {
            case "200×200":
                SelectedWidth = 200;
                SelectedHeight = 200;
                break;
            case "300×300":
                SelectedWidth = 300;
                SelectedHeight = 300;
                break;
            case "400×400":
                SelectedWidth = 400;
                SelectedHeight = 400;
                break;
            case "Fill":
                SelectedWidth = 800;
                SelectedHeight = 500;
                break;
        }
    }

    private void OpenInNewWindow()
    {
        var window = new Window
        {
            Content = new UserControls.TargetSection { DataContext = new TargetSectionViewModel() },
            Width = SelectedWidth,
            Height = SelectedHeight
        };
        window.Show();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}