using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using ReactiveUI;
using RFD.Interfaces;
using RFD.Models;
using RFD.UserControls;

namespace RFD.ViewModels;

public class StatusSectionViewModel
{
    private readonly IWindowService _windowService;

    public StatusSectionViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        OpenInNewWindowCommand = ReactiveCommand.Create(OpenInNewWindow);
    }

    public ReactiveCommand<Unit, Unit> OpenInNewWindowCommand { get; }
    public ObservableCollection<StatusBox> InfoStatusList { get; set; } = [];

    private void OpenInNewWindow()
    {
        var newControl = new StatusSection { DataContext = this };
        
        if (App.Current?.Resources.TryGetResource("Statuses", App.Current?.ActualThemeVariant, out var result1) == true)
        {
            _windowService.OpenWindow(newControl, result1.ToString());
        }
        else
        {
            _windowService.OpenWindow(newControl, "Not found resources");
        }
    }

    public void AddStatusBox(StatusBox statusBox)
    {
        InfoStatusList.Add(statusBox);
    }

    public void ClearStatusBox()
    {
        InfoStatusList.Clear();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}