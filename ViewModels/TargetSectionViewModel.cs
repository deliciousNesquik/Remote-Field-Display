using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RFD.ViewModels;

public partial class TargetSectionViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}