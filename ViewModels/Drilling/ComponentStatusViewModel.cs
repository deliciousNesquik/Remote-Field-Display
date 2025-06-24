using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using Avalonia.Threading;
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
            if (App.Current?.Resources.TryGetResource("Background", App.Current?.ActualThemeVariant, out var background) == true)
            {
                _windowService.OpenWindow(newControl, result1.ToString(), Brush.Parse(background.ToString()));
            }
        }
        else
        {
            if (App.Current?.Resources.TryGetResource("Background", App.Current?.ActualThemeVariant, out var background) == true)
            {
                _windowService.OpenWindow(newControl, "Not found resources", Brush.Parse(background.ToString()));
            } 
        }
    }

    public void AddStatusBox(StatusBox statusBox)
    {
        Dispatcher.UIThread.Post((() => { InfoStatusList.Add(statusBox); }));
    }

    public void ClearStatusBox(string header)
    {
        for (var index = 0; index <= InfoStatusList.Count; index++)
        {
            if (InfoStatusList[index].Header != header) continue;
            var index1 = index;
            Dispatcher.UIThread.Post(() => {InfoStatusList.RemoveAt(index1);});
        }
    }
    
    public void ClearStatusBox()
    {
        Dispatcher.UIThread.Post((() => { InfoStatusList.Clear(); }));
        
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}