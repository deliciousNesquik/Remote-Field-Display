using CommunityToolkit.Mvvm.Input;

namespace RFD.Interfaces;

public interface IDialog
{
    IRelayCommand ConfirmCommand { get; set; }
    IRelayCommand CancelCommand { get; set; }
    
    Action? DialogClose { get; set; }
}