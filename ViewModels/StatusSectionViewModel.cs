using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using ReactiveUI;
using RFD.Interfaces;
using RFD.Models;

namespace RFD.ViewModels;

public class StatusSectionViewModel
{
    private readonly IWindowService _windowService;
    public ReactiveCommand<Unit, Unit> OpenInNewWindowCommand { get; }
    public ObservableCollection<StatusBox> InfoStatusList { get; set; } = [];

    public StatusSectionViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        OpenInNewWindowCommand = ReactiveCommand.Create(OpenInNewWindow);
    }
    
    private void OpenInNewWindow()
    {
        var newControl = new UserControls.StatusSection() { DataContext = this };
        _windowService.OpenWindow(newControl, "Статусы");
    }
    
    public void AddStatusBox(StatusBox statusBox) => InfoStatusList.Add(statusBox);
    public void ClearStatusBox() => InfoStatusList.Clear();
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}