using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using ReactiveUI;
using RFD;
using RFD.Interfaces;
using RFD.Models;
using RFD.UserControls;

namespace RFD.ViewModels;
public sealed class InformationSectionViewModel : ViewModelBase
{
    private readonly IWindowService _windowService;
    private bool _noData = true;

    public InformationSectionViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        OpenInNewWindowCommand = ReactiveCommand.Create(OpenInNewWindow);
        
        // Подписываемся на изменения коллекции
        InfoBlockList.CollectionChanged += (sender, args) => 
        {
            NoData = InfoBlockList.Count == 0;
        };
    }

    public ReactiveCommand<Unit, Unit> OpenInNewWindowCommand { get; }

    public bool NoData
    {
        get => _noData;
        private set
        {
            if (_noData == value) return;
            _noData = value;
            this.RaisePropertyChanged();
        }
    }

    public ObservableCollection<InfoBox> InfoBlockList { get; } = new();

    public void AddInfoBox(InfoBox infoBox)
    {
        Dispatcher.UIThread.Post((() => { InfoBlockList.Add(infoBox); }));
        // NoData обновится автоматически благодаря подписке на CollectionChanged
    }
    
    public void ClearInfoBox(string title)
    {
        for (var index = 0; index <= InfoBlockList.Count; index++)
        {
            if (InfoBlockList[index].Title != title) continue;
            var index1 = index;
            Dispatcher.UIThread.Post((() => {  InfoBlockList.RemoveAt(index1); }));
        }
    }

    public void ClearInfoBox()
    {
        Dispatcher.UIThread.Post((() => {  InfoBlockList.Clear(); }));
        // NoData обновится автоматически благодаря подписке на CollectionChanged
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
}