using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RFD.Models
{
    public partial class InfoBox : ObservableObject
    {
        [ObservableProperty] public string title;
        [ObservableProperty] public string content;
        [ObservableProperty] public string inscription;

        public InfoBox(string title, string content, string inscription = "")
        {
            this.title = title;
            this.content = content;
            this.inscription = inscription;
        }
    }
}