using ReactiveUI;
using RFD.Interfaces;

namespace RFD.Models;

public class InfoBox : ViewModelBase
{
    private string _content;
    private string _inscription;
    private string _title;

    public InfoBox(string title = "", string content = "-", string inscription = "-")
    {
        _title = title;
        _content = content;
        _inscription = inscription;
    }


    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    public string Inscription
    {
        get => _inscription;
        set => this.RaiseAndSetIfChanged(ref _inscription, value);
    }
}