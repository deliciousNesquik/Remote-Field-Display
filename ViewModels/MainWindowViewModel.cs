using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RFD.Models;

namespace RFD.ViewModels
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<InfoBlock> InfoBlockList { get; private set; }
        public ObservableCollection<InfoStatus> InfoStatusList { get; private set; }
        public double MagneticDeclination { get; private set; } 
        public double ToolfaceOffset { get; private set; } 
        
        
        //Свойство которое изменяется с true на false в зависимости от открытия или закрытия окна
        private bool _isModalWindowOpen = false;
        public bool IsModalWindowOpen
        {
            get => _isManualConnectingOpen;
            set {
                _isModalWindowOpen = value;
                if (value)
                {
                    BlurRadius = 10;
                }
                else
                {
                    BlurRadius = 0;
                }
                OnPropertyChanged();
            }
        }

        private bool _isManualConnectingOpen = false;
        public bool IsManualConnectingOpen
        {
            get => _isManualConnectingOpen;
            set
            {
                _isManualConnectingOpen = value;
                IsModalWindowOpen = value;
                OnPropertyChanged();
            }
        }

        private bool _isAutomaticlyConnectingOpen = false;
        public bool IsAutomaticlyConnectingOpen
        {
            get => _isAutomaticlyConnectingOpen;
            set
            {
                _isAutomaticlyConnectingOpen = value;
                IsModalWindowOpen = value;
                OnPropertyChanged();
            }
        }
        
        private double _blurRadius = 0;
        public double BlurRadius
        {
            get => _blurRadius;
            set
            {
                _blurRadius = value;
                OnPropertyChanged();
            }
        }
        
        public ManualConnectionDialogViewModel ManualConnectionDialogViewModel { get; }
        public ICommand OpenManualConnectingCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand OpenAutomaticlyConnectingCommand { get; }

        public MainWindowViewModel() 
        {
            InfoBlockList = [
                new ("Высота блока", "-", "м"),
                new ("Глубина долота", "-", "м"),
                new ("Текущий забой", "-", "м"),
                new ("TVD", "-", "м"),
                new ("Расстояние до забоя", "-", "м"),
                new ("Rop средний", "-", "м/ч"),
                new ("Зенит", "-", "°"),
                new ("Азимут", "-", "°"),
            ];
            InfoStatusList = [
                new ("Клинья", ""),
                new ("Насос", ""),
                new ("Забой", ""),
            ];
            MagneticDeclination = 0.00;
            ToolfaceOffset = 0.00;

            ManualConnectionDialogViewModel = new ManualConnectionDialogViewModel();
            ManualConnectionDialogViewModel.IsOpenAction += OpenCloseManualConnecting;
            
            OpenAutomaticlyConnectingCommand = new RelayCommand(() => IsAutomaticlyConnectingOpen = true,
                () => !IsModalWindowOpen);
            OpenManualConnectingCommand = new RelayCommand(() => IsManualConnectingOpen = true,
                () => !IsModalWindowOpen);
        }

        public void OpenCloseManualConnecting(bool value)
        {
            IsManualConnectingOpen = value;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
