using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace RFD.ViewModels
{
    public class ManualConnectionDialogViewModel
    {
        public static event Action<bool> IsOpenAction;
        
        public string IpAddress { get; set; }
        public string Port { get; set; }

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        private readonly Action _onClose;

        public ManualConnectionDialogViewModel()
        {
            ConfirmCommand = new RelayCommand(() => Confirm(), 
                () => true);
            
            CancelCommand = new RelayCommand(() => Cancel(),
                () => true);
        }

        private void Confirm()
        {
            Console.WriteLine("Confirm = " + false);
            IsOpenAction?.Invoke(false);
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