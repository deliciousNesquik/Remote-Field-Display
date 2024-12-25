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
    private readonly Action? _closeDialog;
    private readonly Action? _userCloseDialog;
    private readonly Action<bool> _connectionStatus;

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
    public AutomaticConnectionDialogViewModel(Action? userCloseDialog, Action<bool> connectionStatus, Action? closeDialog)
    {
        _closeDialog = closeDialog ?? throw new ArgumentNullException(nameof(closeDialog));
        _userCloseDialog = userCloseDialog;
        _connectionStatus = connectionStatus;

        // Подписываемся на событие подключения
        if (_connectionStatus != null)
            _connectionStatus += ConnectionHasSuccess;
        
        // Настраиваем команду для кнопки "Отмена"
        CancelCommand = new RelayCommand(UserCloseDialog);
    }

    private void ConnectionHasSuccess(bool status)
    {
        if (status)
        {
            Connection = true;
            CloseDialog();
        }
        else
        {
            Connection = false;
            CloseDialog();
        }
    }

    private void UserCloseDialog()
    {
        this._userCloseDialog?.Invoke();
        this.CloseDialog();
    }

    private void CloseDialog() => this._closeDialog?.Invoke();
    
    // Реализация INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}