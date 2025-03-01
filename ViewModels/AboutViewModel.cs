using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace RFD.ViewModels;

public class AboutViewModel
{
    public ICommand CloseCommand { get; }
    public Action? CloseDialog;

    public AboutViewModel()
    {
        CloseCommand = new RelayCommand(Close);
    }
    
    private void Close() => CloseDialog?.Invoke();
}