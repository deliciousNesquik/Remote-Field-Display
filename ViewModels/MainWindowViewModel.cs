using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RFD.Models;
using RFD.UserControls;
using ReactiveUI;

namespace RFD.ViewModels
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {

        #region Переменные: Геофизические параметры

        public ObservableCollection<InfoBlock> InfoBlockList { get; private set; }
        public ObservableCollection<InfoStatus> InfoStatusList { get; private set; }
        public double MagneticDeclination { get; private set; } 
        public double ToolfaceOffset { get; private set; } 

        #endregion

        #region Переменные: Параметры отвечающие за работу модальных окон поверх главного окна

        private UserControl _currentUserControl;
        public UserControl CurrentUserControl
        {
            get => _currentUserControl;
            set
            {
                _currentUserControl = value;
                OnPropertyChanged();
            }
        }
        
        private bool _isModalWindowOpen = false;
        public bool IsModalWindowOpen
        {
            get => _isModalWindowOpen;
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

        private bool _isAutomaticConnectingOpen = false;
        public bool IsAutomaticConnectingOpen
        {
            get => _isAutomaticConnectingOpen;
            set
            {
                _isAutomaticConnectingOpen = value;
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

        #endregion

        #region Переменные: View Models модальных окон

        public ManualConnectionDialogViewModel ManualConnectionDialogViewModel { get; set; }
        public AutomaticConnectionDialogViewModel AutomaticConnectionDialogViewModel { get; set; }
        
        public ConnectionControlViewModel ConnectionControlViewModel { get; set; }

        #endregion

        #region Переменные: Команды основного меню

        public ICommand OpenAutomaticConnectingCommand { get; }
        public ICommand OpenManualConnectingCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand SettingsCommand { get; }

        #endregion

        #region Переменные: Параметры соединения с сервером

        private string _ipAddress;

        public String IpAddress
        {
            get => _ipAddress;
            set
            {
                _ipAddress = value;
                OnPropertyChanged();
            }
        }
        public SolidColorBrush ConnectionStatus { get; set; }

        #endregion

        public MainWindowViewModel() 
        {
            //Геофизические параметры заполнены для примера
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
            
            //ConnectionStatus = SolidColorBrush.Parse("#00b300");
            ConnectionControlViewModel = ConnectionControlViewModel.Current;
            
            //Команды основного меню
            OpenAutomaticConnectingCommand = new RelayCommand(() => OpenAutomaticConnecting(), () => !IsModalWindowOpen);
            OpenManualConnectingCommand = new RelayCommand(() => OpenManualConnecting(), () => !IsModalWindowOpen);
        }

        #region Методы: Методы для открытия окон соединения с сервером

        /*Метод для открытия ручного окна соединения*/
        public void OpenManualConnecting()
        {
            //Получаем view model ручного окна подключения для того чтобы отслеживать статус подключения
            ManualConnectionDialogViewModel = new ManualConnectionDialogViewModel();
            //Добавляем триггер который проверяет что происходит во время ручного подключения
            ManualConnectionDialogViewModel.IsOpenAction += TriggerCloseManualConnecting;
            
            //В DataTemplate передаем модальное окно для его отображения
            CurrentUserControl = new ManualConnectionDialog();
            
            //Указываем что параметр открото ли окно ручного соединения равно правде
            IsManualConnectingOpen = true;
        }
        
        /*Метод для открытия автоматического окна соединения*/
        public void OpenAutomaticConnecting()
        {
            //Получаем view model ручного окна подключения для того чтобы отслеживать статус подключения
            AutomaticConnectionDialogViewModel = new AutomaticConnectionDialogViewModel();
            //Добавляем триггер который проверяет что происходит во время ручного подключения
            AutomaticConnectionDialogViewModel.IsOpenAction += TriggerCloseAutomaticConnecting;
            
            //В DataTemplate передаем модальное окно для его отображения
            CurrentUserControl = new AutomaticConnectingDialog();
            
            //Указываем что параметр открото ли окно ручного соединения равно правде
            IsAutomaticConnectingOpen = true;
        }

        #endregion

        #region Методы: Триггеры на закрытие окон соединения с сервером

        /*Тригер для отслеживания статуса закрытия ручного окна соединения*/
        public void TriggerCloseManualConnecting(bool value)
        {
            IsManualConnectingOpen = value;
        }
        
        /*Тригер для отслеживания статуса закрытия автоматического окна соединения*/
        public void TriggerCloseAutomaticConnecting(bool value)
        {
            IsManualConnectingOpen = value;
        }

        #endregion

        #region Доп. методы

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
