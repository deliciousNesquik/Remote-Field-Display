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
    
    #region Переменные: Для связи между App.xaml.cs и текущим файлом
    private RFD.App Model => App.Current as RFD.App;
    #endregion
    
    public ICommand CancelCommand { get; }

    public AutomaticConnectionDialogViewModel()
    {
        CancelCommand = new RelayCommand(() => Cancel(), () => true);
            
        AutoConnect();
    }
    private void Cancel()
    {
        IsOpenAction?.Invoke(false);
    }
    
    public void AutoConnect()
    {
        if (Model != null) Model.AutoConnect();
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}