using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RFD.Models
{
    public class InfoBox : INotifyPropertyChanged
    {
        public string Title { get; set; }

        private string _content;
        public string Content 
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Inscription { get; set; }

        public InfoBox(string title, object content, string inscription = "")
        {
            Title = title;
            Content = FormatContent(content);
            Inscription = inscription;
        }

        private static string FormatContent(object content)
        {
            return content switch
            {
                double d => d.ToString("F2", System.Globalization.CultureInfo.CurrentCulture),
                float f => f.ToString("F2", System.Globalization.CultureInfo.CurrentCulture),
                int i => i.ToString(System.Globalization.CultureInfo.CurrentCulture),
                string s => TruncateString(s, 5),
                _ => content.ToString() ?? string.Empty
            };
        }

        private static string TruncateString(string text, int maxLength)
        {
            return text.Length > maxLength ? string.Concat(text.AsSpan(0, maxLength), "...") : text;
        }

        public override string ToString() => $"{Title}: {Content} ({Inscription})";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}