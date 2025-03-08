using ReactiveUI;
using RFD.Interfaces;

namespace RFD.Models
{
    public class InfoBox : ViewModelBase
    {
        private string _title;
        private string _content;
        private string _inscription;
        
        
        public string Title {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }
        public string Content {
            get => _content;
            set => this.RaiseAndSetIfChanged(ref _content, value);
        }

        public string Inscription {
            get => _inscription;
            set => this.RaiseAndSetIfChanged(ref _inscription, value);
        }

        public InfoBox(string title = "", string content = "-", string inscription = "-") {
            this._title = title;
            this._content = content;
            this._inscription = inscription;
        }
    }
}