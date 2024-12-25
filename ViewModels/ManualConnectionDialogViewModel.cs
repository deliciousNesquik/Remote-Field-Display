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
        private Action? _closeDialog;
        private Action<string> _tryConnect;
        private Action<bool> _statusConnect;
        
        public string IpAddress { get; set; }
        public bool IsConnected { get; set; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public ManualConnectionDialogViewModel(Action<string> tryConnect, Action<bool> statusConnect, Action? closeDialog)
        {
            this._tryConnect += ipadress =>
            {
                tryConnect?.Invoke(ipadress);
            };
            this._closeDialog += () =>
            {
                closeDialog?.Invoke();
            };
                
            statusConnect += status =>
            {
                if (status)
                {
                    this.IsConnected = false;
                    this.CloseDialog();
                }
            };

            this.IsConnected = false;    
            
            ConfirmCommand = new RelayCommand(() => this.Confirm(), () => true);
            CancelCommand = new RelayCommand(() => this.CloseDialog(), () => true);
        }

        private void Confirm()
        {
            var pattern = @"^((25[0-5]|2[0-4][0-9]|1[0-9]{1,2}|[1-9]?[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9]{1,2}|[1-9]?[0-9])$";
            if (Regex.IsMatch(this.IpAddress, pattern))
            {
                this._tryConnect.Invoke(this.IpAddress);
                this.IsConnected = true;
            }
        }

        private void CloseDialog()
        {
            Console.WriteLine("CloseDialog");
            _closeDialog?.Invoke();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}