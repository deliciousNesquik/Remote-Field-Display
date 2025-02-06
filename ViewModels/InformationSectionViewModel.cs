using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RFD.Models;

namespace RFD.ViewModels;

public class InformationSectionViewModel : INotifyPropertyChanged
{
    public ObservableCollection<InfoBox> InfoBlockList { get; private set; }

    
    public InformationSectionViewModel()
    {
        InfoBlockList = [
            new ("Высота блока", "-", "м"),
            new ("Глубина долота", "-", "м"),
            new ("Текущий забой", "-", "м"),
            new ("TVD", "-", "м"),
            new ("Расстояние до забоя", "-", "м"),
            new ("Rop средний", "-", "м/ч"),
            new ("Зенит", "-", "°"),
            new ("Азимут", "-", "°"),
        ];
    }

    public void AddInfoBox(InfoBox infoBox)
    {
        InfoBlockList.Add(infoBox);
    }
    public void AddInfoBox(List<InfoBox> infoBoxList)
    {
        foreach (InfoBox infoBox in infoBoxList)
        {
            AddInfoBox(infoBox);
        }
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