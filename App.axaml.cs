using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RFD.ViewModels;
using RFD.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using ExCSS;
using NPFGEO.LWD.Net;
using RFD.Models;
using DateTime = System.DateTime;

namespace RFD;

public class App : Application
{
    public static App Instance => (App)Current!;
    private MainWindowViewModel _mainWindowViewModel = null!;
    public NetworkService NetworkService = null!;
    
    public event Action<string>? ThemeChanged;
    

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        if (Design.IsDesignMode) return;

        NetworkService = new NetworkService();
        NetworkService.SettingsReceived += OnSettingsReceived;
        NetworkService.DataReceived += OnDataReceived;
        NetworkService.Disconnected += OnDisconnected;
        NetworkService.ConnectedStatusChanged += OnConnectedStatusChanged;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.Exit += OnExit;
                this.GetObservable(ActualThemeVariantProperty).Subscribe(OnThemeChanged);

                _mainWindowViewModel = new MainWindowViewModel();
                MainWindow mainWindow = new() { DataContext = _mainWindowViewModel };
                desktop.MainWindow = mainWindow;
                break;
        }
        base.OnFrameworkInitializationCompleted();
    }

    protected void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        NetworkService.Dispose();
        NetworkService.SettingsReceived -= OnSettingsReceived;
        NetworkService.DataReceived -= OnDataReceived;
        NetworkService.Disconnected -= OnDisconnected;
        NetworkService.ConnectedStatusChanged -= OnConnectedStatusChanged;
    }
    
    
    private void OnSettingsReceived(Settings settings)
    {
        Dispatcher.UIThread.InvokeAsync(() => SetSettings(settings));
    }

    private void OnDataReceived(DataObject data)
    {
        Dispatcher.UIThread.InvokeAsync(() => SetData(data));
    }

    private void OnDisconnected()
    {
        Dispatcher.UIThread.InvokeAsync(() => _mainWindowViewModel.OnConnectionStateChanged());
    }

    private void OnConnectedStatusChanged()
    {
        Dispatcher.UIThread.InvokeAsync(() => _mainWindowViewModel.OnConnectionStateChanged());
    }
    
    
    
    private void OnThemeChanged(ThemeVariant newTheme)
    {
        ThemeChanged?.Invoke(newTheme.Key.ToString());
    }
    
    
    private void SetSettings(Settings settings)
    {
        _mainWindowViewModel.InformationSectionViewModel.ClearInfoBox();
        _mainWindowViewModel.StatusSectionViewModel.ClearStatusBox();
        
        foreach (var flag in settings.Flags)
        { _mainWindowViewModel.StatusSectionViewModel.AddStatusBox(new StatusBox(flag.Name, false)); }

        foreach (var flag in settings.Statuses)
        { _mainWindowViewModel.StatusSectionViewModel.AddStatusBox(new StatusBox(flag.Name, false)); }

        foreach (var param in settings.Parameters)
        { _mainWindowViewModel.InformationSectionViewModel.AddInfoBox(new InfoBox(param.Name, "-", param.Units)); }
        
        _mainWindowViewModel.ParametersSectionViewModel.MagneticDeclination = settings.InfoParameters.MagneticDeclination;
        _mainWindowViewModel.ParametersSectionViewModel.ToolfaceOffset = settings.InfoParameters.ToolfaceOffset;

        _mainWindowViewModel.TargetSectionViewModel.FromCenterToBorder = settings.Target.FromCenterToBorder;
        _mainWindowViewModel.TargetSectionViewModel.Capacity = settings.Target.Capacity;
        _mainWindowViewModel.TargetSectionViewModel.IsHalfMode = settings.Target.IsHalfMode;
        _mainWindowViewModel.TargetSectionViewModel.GridFrequency = settings.Target.GridFrequency;
        _mainWindowViewModel.TargetSectionViewModel.FontSize = settings.Target.FontSize;
        (_mainWindowViewModel.TargetSectionViewModel.RingWidth, _mainWindowViewModel.TargetSectionViewModel.RingThickness) = (settings.Target.RingWidth, settings.Target.RingWidth);
        _mainWindowViewModel.TargetSectionViewModel.DefaultRadius = settings.Target.DefaultRadius;
        _mainWindowViewModel.TargetSectionViewModel.ReductionFactor = settings.Target.ReductionFactor;
        Console.WriteLine(settings.Target.ReductionFactor);
        
        // Для установки конкретной темы
        _mainWindowViewModel.SwitchTheme(settings.ThemeStyle.ToString()[..(settings.ThemeStyle.ToString().Length - 5)]);
        
        _mainWindowViewModel.TargetSectionViewModel.SetSector(
            startAngle: settings.Target.SectorDirection - (settings.Target.SectorWidth / 2), 
            endAngle: settings.Target.SectorDirection + (settings.Target.SectorWidth / 2)
            );
    }

    private void SetData(DataObject data)
    {
        var targetPoints = data.TargetPoints.ToList();
        for (var i = 0; i < targetPoints.Count; i++)
        {
            _mainWindowViewModel.ParametersSectionViewModel.Angle = targetPoints[i].Angle;
            _mainWindowViewModel.ParametersSectionViewModel.ToolfaceType = targetPoints[i].ToolfaceType.ToString();
            
            if (_mainWindowViewModel.TargetSectionViewModel.FromCenterToBorder)
            {
                _mainWindowViewModel.TargetSectionViewModel.SetPoint(
                    index: _mainWindowViewModel.TargetSectionViewModel.Capacity - i - 1,
                    angle: targetPoints[i].Value);
            }
            else
            {
                _mainWindowViewModel.TargetSectionViewModel.SetPoint(
                    index: _mainWindowViewModel.TargetSectionViewModel.Capacity - targetPoints.Count + i,
                    angle: targetPoints[i].Value);
            }
        }

        foreach (var t in data.Flags)
        { foreach (var t2 in _mainWindowViewModel.StatusSectionViewModel.InfoStatusList)
            { if (t2.Header == t.Name)
                { t2.Status = t.Value;
                }
            }
        }

        foreach (var t in data.Statuses)
        { foreach (var t2 in _mainWindowViewModel.StatusSectionViewModel.InfoStatusList)
            { if (t2.Header == t.Name)
                { t2.Status = Convert.ToBoolean(t.Value);
                }
            }
        }

        foreach (var t in data.Parameters)
        { foreach (var t2 in _mainWindowViewModel.InformationSectionViewModel.InfoBlockList)
            { if (t2.Title == t.Name)
                { t2.Content = double.Round(t.Value, 2).ToString(CultureInfo.CurrentCulture); }
            }
        }
           
        _mainWindowViewModel.ParametersSectionViewModel.SetTime(data);
    }
}