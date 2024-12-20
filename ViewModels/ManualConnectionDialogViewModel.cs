using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace RFD.ViewModels
{
    public class ManualConnectionDialogViewModel
    {
        public static event Action<bool> IsOpenAction;
        
        public string IpAddress { get; set; }

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public ManualConnectionDialogViewModel()
        {
            ConfirmCommand = new RelayCommand(() => Confirm(), 
                () => true);
            
            CancelCommand = new RelayCommand(() => Cancel(),
                () => true);
        }

        private void Confirm()
        {
            var pattern = @"^((25[0-5]|2[0-4][0-9]|1[0-9]{1,2}|[1-9]?[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9]{1,2}|[1-9]?[0-9])$";
            if (Regex.IsMatch(IpAddress, pattern))
            {
                //TODO
                //Подключение к серверу
                
                IsOpenAction?.Invoke(false);
            }
        }
        private void Cancel()
        {
            Console.WriteLine("Cancel = " + false);
            IsOpenAction?.Invoke(false);
        }
        
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}