using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace RFD.ViewModels;

public class ManualConnectionDialogViewModel
{
    /// <summary>Триггер для отлавливания закрытия диалогового окна</summary>
    public Action? CloseDialog;

    /// <summary>Триггер оповещает родителя о том, что диалоговое окно хочет выполнить попытку подключения</summary>
    public Action<string>? ConnectionAttempt;

    /// <summary>Триггер, который необходимо вызывать в родителе, чтобы уведомить диалоговое окно о том что соединение успешно</summary>
    public Action<bool> ConnectionStatus;

    public ManualConnectionDialogViewModel()
    {
        IsActionInProgress = false;

        ConnectionStatus += statusConnection =>
        {
            if (statusConnection) CloseDialog?.Invoke();
        };

        ConfirmCommand = new RelayCommand(() => Confirm(), () => !IsActionInProgress);
        CancelCommand = new RelayCommand(() => Close(), () => !IsActionInProgress);
    }

    public string? FieldIpАddress { get; set; }
    public bool IsActionInProgress { get; set; }
    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }

    private void Confirm()
    {
        if (string.IsNullOrEmpty(FieldIpАddress)) return;

        var pattern =
            @"^((25[0-5]|2[0-4][0-9]|1[0-9]{1,2}|[1-9]?[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9]{1,2}|[1-9]?[0-9])$";
        if (Regex.IsMatch(FieldIpАddress, pattern))
        {
            ConnectionAttempt?.Invoke(FieldIpАddress);
            IsActionInProgress = true;
        }
    }

    private void Close()
    {
        IsActionInProgress = false;
        CloseDialog?.Invoke();
    }
}