using System.Reactive;
using ReactiveUI;

namespace RFD.Interfaces;

public interface IDialog
{
    ReactiveCommand<Unit, Unit> ConfirmCommand { get; set; }
    ReactiveCommand<Unit, Unit> CancelCommand { get; set; }

    Action? DialogClose { get; set; }
}