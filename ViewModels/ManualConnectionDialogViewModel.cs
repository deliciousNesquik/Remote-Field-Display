using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace RFD.ViewModels
{
    public class ManualConnectionDialogViewModel
    {
        /// <summary>Триггер для отлавливания закрытия диалогового окна</summary>
        public Action? CloseDialog;
        
        /// <summary>Триггер оповещает родителя о том, что диалоговое окно хочет выполнить попытку подключения</summary>
        public Action<string>? ConnectionAttempt;
        
        /// <summary>Триггер, который необходимо вызывать в родителе чтобы уведомить диалоговое окно о том что соединение успешно</summary>
        public Action<bool> ConnectionStatus;
        
        public string? FieldIpАddress { get; set; }
        public bool IsActionInProgress { get; set; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public ManualConnectionDialogViewModel()
        {
            IsActionInProgress = false;

            ConnectionStatus += statusConnection =>
            {
                if (statusConnection) CloseDialog();
            };
            
            ConfirmCommand = new RelayCommand(() => this.Confirm(), () => !IsActionInProgress);
            CancelCommand = new RelayCommand(() => this.Close(), () => !IsActionInProgress);
        }
        private void Confirm()
        {
            if (string.IsNullOrEmpty(this.FieldIpАddress))
            {
                return;
            }

            var pattern = @"^((25[0-5]|2[0-4][0-9]|1[0-9]{1,2}|[1-9]?[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9]{1,2}|[1-9]?[0-9])$";
            if (Regex.IsMatch(this.FieldIpАddress, pattern))
            {
                ConnectionAttempt?.Invoke(this.FieldIpАddress);
                IsActionInProgress = true;
            }
            else
            {
                //Console.WriteLine("Invalid IP address format");
            }
        }

        private void Close()
        {
            IsActionInProgress = false;
            CloseDialog?.Invoke();
        }
    }
}