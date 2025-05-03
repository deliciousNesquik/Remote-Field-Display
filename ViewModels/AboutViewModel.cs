using System.Reactive;
using ReactiveUI;

namespace RFD.ViewModels;

public class AboutViewModel
{
    public Action? CloseDialog;

    public AboutViewModel()
    {
        CloseCommand = ReactiveCommand.Create(Close);
    }

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    private void Close()
    {
        CloseDialog?.Invoke();
    }
}