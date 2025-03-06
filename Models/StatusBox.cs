using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RFD.Models
{
    public partial class StatusBox : ObservableObject
    {
        [ObservableProperty] public string header;
        [ObservableProperty] public bool status;
        public StatusBox(string header, bool status)
        {
            this.header = header;
            this.status = status;
        }
    }
}
