using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReactiveUI;
using RFD.Interfaces;
using RFD.Models;
using System.Reactive;

namespace RFD.ViewModels;

public sealed class InformationSectionViewModel : INotifyPropertyChanged
{
    private readonly IWindowService _windowService;
    public ReactiveCommand<Unit, Unit> OpenInNewWindowCommand { get; }


    public ObservableCollection<InfoBox> InfoBlockList { get; set; } = [];

    public void AddInfoBox(InfoBox infoBox) => InfoBlockList.Add(infoBox);
    public void ClearInfoBox() => InfoBlockList.Clear();

    public InformationSectionViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        OpenInNewWindowCommand = ReactiveCommand.Create(OpenInNewWindow);
    }
    private void OpenInNewWindow()
    {
        var newControl = new UserControls.InformationSection() { DataContext = this };
        _windowService.OpenWindow(newControl, "Информация");
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
