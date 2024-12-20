using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace RFD.ViewModels;

public class AutomaticConnectionDialogViewModel
{
    public static event Action<bool> IsOpenAction;
    public ICommand CancelCommand { get; }

    public AutomaticConnectionDialogViewModel()
    {
        CancelCommand = new RelayCommand(() => Cancel(), () => true);
    }
    
    private void Cancel()
    {
        IsOpenAction?.Invoke(false);
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}