using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using ReactiveUI;
using RFD.Interfaces;
using RFD.Models;
using RFD.UserControls;

namespace RFD.ViewModels;

public sealed class InformationSectionViewModel : INotifyPropertyChanged
{
    private readonly IWindowService _windowService;
    private bool _noData = true;

    public InformationSectionViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        OpenInNewWindowCommand = ReactiveCommand.Create(OpenInNewWindow);
    }

    public ReactiveCommand<Unit, Unit> OpenInNewWindowCommand { get; }

    public bool NoData
    {
        get => _noData;
        set => _noData = value;
    }


    public ObservableCollection<InfoBox> InfoBlockList { get; set; } = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    public void AddInfoBox(InfoBox infoBox)
    {
        NoData = false;
        InfoBlockList.Add(infoBox);
    }

    public void ClearInfoBox()
    {
        InfoBlockList.Clear();
        NoData = true;
        
    }

    private void OpenInNewWindow()
    {
        var newControl = new InformationSection { DataContext = this };
        
        if (App.Current?.Resources.TryGetResource("Information", App.Current?.ActualThemeVariant, out var result1) == true)
        {
            _windowService.OpenWindow(newControl, result1.ToString());
        }
        else
        {
            _windowService.OpenWindow(newControl, "Not found resources");
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}