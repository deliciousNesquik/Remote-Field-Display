using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RFD.Models;

namespace RFD.ViewModels;

public class InformationSectionViewModel : INotifyPropertyChanged
{
    public ObservableCollection<InfoBox> InfoBlockList { get; set; } = new();


    public void AddInfoBox(InfoBox infoBox)
    {
        InfoBlockList.Add(infoBox);
    }
    public void ClearInfoBox()
    {
        InfoBlockList.Clear();
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}