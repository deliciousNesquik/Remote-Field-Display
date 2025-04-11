using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace RFD.ViewModels;

public class AboutViewModel
{
    public Action? CloseDialog;

    public AboutViewModel()
    {
        CloseCommand = new RelayCommand(Close);
    }

    public ICommand CloseCommand { get; }

    private void Close()
    {
        CloseDialog?.Invoke();
    }
}