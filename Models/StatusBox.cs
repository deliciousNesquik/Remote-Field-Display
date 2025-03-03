using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RFD.Models
{
    public class StatusBox
    {
        public string Header { get; set; }
        
        private bool _status;
        public bool Status 
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }
        public StatusBox(string header, bool status)
        {
            Header = header;
            Status = status;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
