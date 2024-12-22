using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using DynamicData.Kernel;

namespace RFD.ViewModels;

public class ConnectionControlViewModel
{
    public static ConnectionControlViewModel Current = new();
    private RFD.App Model => App.Current as RFD.App;
    public bool IsConnected => Model.Connected;

    public string CurrentIpAddress => Model.CurrentIpAddress;
    public string ManualConnectAddress { set; get; }
    public void AutoConnect() { Model.AutoConnect(); }
    public void Connect()
    {
        //vSetIPAddress vSetIPAddress = new vSetIPAddress();
        //vSetIPAddress.DataContext = Current;
        //vSetIPAddress.Owner = Application.Current.MainWindow;
        //var result = vSetIPAddress.ShowDialog();
        //if (result != null && result != false)
        //{
        //    Model.Connect(ManualConnectAddress);
        //    Update();
        //}
    }

    public void Disconnect() { Model.Disconnect(); }
    public void Update()
    {
        Console.WriteLine(IsConnected);
        Console.WriteLine(CurrentIpAddress);
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(CurrentIpAddress));
    }
    
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}