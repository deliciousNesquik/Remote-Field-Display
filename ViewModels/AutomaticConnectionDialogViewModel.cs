using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using CommunityToolkit.Mvvm.Input;

namespace RFD.ViewModels;
public class AutomaticConnectionDialogViewModel
{
    /// <summary>Триггер для отлавливания закрытия диалогового окна</summary>
    public Action? CloseDialog;
    
    /// <summary>Триггер который вызывается если пользователь закроет диалоговое окно</summary>
    public Action? UserCloseDialog;
    
    /// <summary>Триггер, который необходимо вызывать в родителе чтобы уведомить диалоговое окно о том что соединение успешно</summary>
    public Action<bool> ConnectionStatus;

    private bool _connection;
    public bool Connection
    {
        get => _connection;
        set
        {
            _connection = value;
            OnPropertyChanged();
        }
    }
    public ICommand CancelCommand { get; }
    public AutomaticConnectionDialogViewModel()
    {
        ConnectionStatus += statusConnection =>
        {
            if (statusConnection)
            {
                Connection = true;
                Close();
            }
            else
            {
                Connection = false;
                Close();
            }
        };
        
        CancelCommand = new RelayCommand(UserClose);
    }

    private void UserClose()
    {
        UserCloseDialog?.Invoke();
        Close();
    }

    private void Close() => CloseDialog?.Invoke();
    

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}